/*
 * BIBLIOTECA VIRTUAL - INTEGRAÇÃO COM API BACKEND
 * Conecta o frontend com a API .NET em https://localhost:5001
 * Gerencia: autenticação JWT, refresh tokens, chamadas HTTP
 */

// ===========================================
// CONFIGURAÇÃO DA API
// ===========================================

const API_CONFIG = {
    baseURL: 'https://localhost:5001/api',
    endpoints: {
        // Autenticação
        login: '/login',
        loginGoogle: '/login/google',
        loginGoogleCallback: '/login/google-callback',
        register: '/users',
        confirmEmail: '/users/confirm/email',
        refreshToken: '/token/refresh-token',
        
        // Livros
        books: '/books',
        booksPaginated: (page, perPage) => `/books/filter?page=${page}&perPage=${perPage}`,
        getBook: (id) => `/books/${id}`,
        updateBook: (id) => `/books/${id}`,
        deleteBook: (id) => `/books/${id}`,
        
        // Comentários
        getBookComments: (bookId, page, perPage) => `/books/${bookId}/comments?page=${page}&perPage=${perPage}`,
        createComment: (bookId) => `/books/${bookId}/comments`,
        getComment: (commentId) => `/comments/${commentId}`,
        deleteComment: (commentId) => `/comments/${commentId}`,
        
        // Likes
        likeBook: (bookId) => `/books/${bookId}/like`,
        unlikeBook: (bookId) => `/books/${bookId}/unlike`
    },
    storage: {
        accessToken: 'access_token',
        refreshToken: 'refresh_token',
        refreshExpiration: 'refresh_expiration',
        userData: 'user_data'
    }
};

// ===========================================
// CLASSE DE GERENCIAMENTO DE TOKENS
// ===========================================

class TokenManager {
    static saveTokens(accessToken, refreshToken, refreshExpiration) {
        localStorage.setItem(API_CONFIG.storage.accessToken, accessToken);
        localStorage.setItem(API_CONFIG.storage.refreshToken, refreshToken);
        localStorage.setItem(API_CONFIG.storage.refreshExpiration, refreshExpiration);
    }

    static getAccessToken() {
        return localStorage.getItem(API_CONFIG.storage.accessToken);
    }

    static getRefreshToken() {
        return localStorage.getItem(API_CONFIG.storage.refreshToken);
    }

    static isRefreshTokenExpired() {
        const expiration = localStorage.getItem(API_CONFIG.storage.refreshExpiration);
        if (!expiration) return true;
        return new Date(expiration) <= new Date();
    }

    static clearTokens() {
        localStorage.removeItem(API_CONFIG.storage.accessToken);
        localStorage.removeItem(API_CONFIG.storage.refreshToken);
        localStorage.removeItem(API_CONFIG.storage.refreshExpiration);
    }

    static saveUserData(userData) {
        localStorage.setItem(API_CONFIG.storage.userData, JSON.stringify(userData));
    }

    static getUserData() {
        const data = localStorage.getItem(API_CONFIG.storage.userData);
        return data ? JSON.parse(data) : null;
    }

    static clearUserData() {
        localStorage.removeItem(API_CONFIG.storage.userData);
    }
}

// ===========================================
// CLIENTE HTTP COM INTERCEPTORS
// ===========================================

class ApiClient {
    static async request(endpoint, options = {}) {
        const url = API_CONFIG.baseURL + endpoint;
        
        const token = TokenManager.getAccessToken();
        if (token && !options.skipAuth) {
            options.headers = {
                ...options.headers,
                'Authorization': `Bearer ${token}`
            };
        }

        if (options.body && typeof options.body === 'object' && !(options.body instanceof FormData)) {
            options.headers = {
                ...options.headers,
                'Content-Type': 'application/json'
            };
            options.body = JSON.stringify(options.body);
        }

        try {
            const response = await fetch(url, options);

            if (response.status === 401 && !options.skipAuth && !options.isRefreshRequest) {
                const refreshed = await this.refreshAccessToken();
                if (refreshed) {
                    return this.request(endpoint, options);
                } else {
                    this.handleUnauthorized();
                    throw new Error('Sessão expirada. Faça login novamente.');
                }
            }

            const data = await this.handleResponse(response);
            return { success: true, data };

        } catch (error) {
            console.error('Erro na requisição:', error);
            throw error;
        }
    }

    static async handleResponse(response) {
        const contentType = response.headers.get('content-type');
        const isJson = contentType && contentType.includes('application/json');

        if (response.status === 204) {
            return null;
        }

        let data = null;
        if (isJson) {
            data = await response.json();
        } else {
            data = await response.text();
        }

        if (!response.ok) {
            const error = this.parseError(data, response.status);
            throw error;
        }

        return data;
    }

    static parseError(data, status) {
        if (data && data.errors && Array.isArray(data.errors)) {
            return new Error(data.errors.join('\n'));
        }

        if (data && data.message) {
            return new Error(data.message);
        }

        const defaultMessages = {
            400: 'Requisição inválida. Verifique os dados enviados.',
            401: 'Não autorizado. Faça login novamente.',
            403: 'Acesso negado.',
            404: 'Recurso não encontrado.',
            500: 'Erro interno do servidor. Tente novamente mais tarde.'
        };

        return new Error(defaultMessages[status] || 'Erro desconhecido na requisição.');
    }

    static async refreshAccessToken() {
        const refreshToken = TokenManager.getRefreshToken();

        if (!refreshToken || TokenManager.isRefreshTokenExpired()) {
            return false;
        }

        try {
            const response = await this.request(
                API_CONFIG.endpoints.refreshToken,
                {
                    method: 'POST',
                    body: { refreshToken },
                    skipAuth: true,
                    isRefreshRequest: true
                }
            );

            if (response.success && response.data) {
                TokenManager.saveTokens(
                    response.data.accessToken,
                    response.data.refreshToken,
                    response.data.refreshTokenExpiration
                );
                return true;
            }

            return false;

        } catch (error) {
            console.error('Erro ao renovar token:', error);
            return false;
        }
    }

    static handleUnauthorized() {
        TokenManager.clearTokens();
        TokenManager.clearUserData();
        
        if (window.location.pathname.includes('index.html') || window.location.pathname === '/') {
            if (typeof showLoginModal === 'function') {
                showLoginModal();
            }
        }
    }

    static get(endpoint, options = {}) {
        return this.request(endpoint, { ...options, method: 'GET' });
    }

    static post(endpoint, body, options = {}) {
        return this.request(endpoint, { ...options, method: 'POST', body });
    }

    static put(endpoint, body, options = {}) {
        return this.request(endpoint, { ...options, method: 'PUT', body });
    }

    static delete(endpoint, options = {}) {
        return this.request(endpoint, { ...options, method: 'DELETE' });
    }
}

// ===========================================
// SERVIÇO DE AUTENTICAÇÃO
// ===========================================

class AuthService {
    static async login(email, password) {
        try {
            const response = await ApiClient.post(
                API_CONFIG.endpoints.login,
                { email, password },
                { skipAuth: true }
            );

            if (response.success && response.data) {
                TokenManager.saveTokens(
                    response.data.accessToken,
                    response.data.refreshToken,
                    response.data.refreshTokenExpiration
                );

                const userData = this.decodeToken(response.data.accessToken);
                TokenManager.saveUserData(userData);

                return { success: true, user: userData };
            }

            return { success: false, error: 'Resposta inválida do servidor' };

        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    static async register(userName, email, password) {
        try {
            const response = await ApiClient.post(
                API_CONFIG.endpoints.register,
                { userName, email, password },
                { skipAuth: true }
            );

            if (response.success && response.data) {
                return { 
                    success: true, 
                    user: response.data,
                    message: 'Conta criada! Verifique seu email para confirmar o cadastro.'
                };
            }

            return { success: false, error: 'Resposta inválida do servidor' };

        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    static async confirmEmail(email, token) {
        try {
            const response = await ApiClient.get(
                `${API_CONFIG.endpoints.confirmEmail}?email=${encodeURIComponent(email)}&token=${encodeURIComponent(token)}`,
                { skipAuth: true }
            );

            return { success: true };

        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    static loginWithGoogle() {
        window.location.href = API_CONFIG.baseURL + API_CONFIG.endpoints.loginGoogle;
    }

    static logout() {
        TokenManager.clearTokens();
        TokenManager.clearUserData();
    }

    static isAuthenticated() {
        const token = TokenManager.getAccessToken();
        const userData = TokenManager.getUserData();
        
        if (!token || !userData) {
            return false;
        }

        try {
            const decoded = this.decodeToken(token);
            const currentTime = Date.now() / 1000;
            
            if (decoded.exp && decoded.exp < currentTime) {
                if (!TokenManager.isRefreshTokenExpired()) {
                    return true;
                }
                return false;
            }

            return true;
        } catch (error) {
            return false;
        }
    }

    static getCurrentUser() {
        if (!this.isAuthenticated()) {
            return null;
        }
        return TokenManager.getUserData();
    }

    static decodeToken(token) {
        try {
            const parts = token.split('.');
            if (parts.length !== 3) {
                throw new Error('Token JWT inválido');
            }

            const payload = parts[1];
            const decoded = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));

            return {
                userId: decoded[Object.keys(decoded).find(k => k.includes('sid'))] || decoded.sid,
                email: decoded.email || decoded[Object.keys(decoded).find(k => k.includes('email'))],
                userName: decoded.name || decoded.userName || decoded[Object.keys(decoded).find(k => k.includes('name'))],
                exp: decoded.exp,
                iat: decoded.iat
            };
        } catch (error) {
            console.error('Erro ao decodificar token:', error);
            return null;
        }
    }
}

// ===========================================
// SERVIÇO DE LIVROS
// ===========================================

class BookService {
    /**
     * Busca livros com paginação
     * @param {number} page - Número da página (começa em 1)
     * @param {number} perPage - Quantidade de livros por página
     * @returns {Promise<Object>} Resposta paginada com livros
     */
    static async getBooksPaginated(page = 1, perPage = 12) {
        try {
            const response = await ApiClient.get(
                API_CONFIG.endpoints.booksPaginated(page, perPage),
                { skipAuth: true }
            );
            return { success: true, data: response.data };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    static async createBook(title, description, file, cover, categoriesIds) {
        try {
            const formData = new FormData();
            formData.append('Title', title);
            formData.append('Description', description);
            formData.append('File', file);
            
            if (cover) {
                formData.append('Cover', cover);
            }
            
            categoriesIds.forEach(id => {
                formData.append('CategoriesIds', id);
            });

            const response = await ApiClient.post(
                API_CONFIG.endpoints.books,
                formData
            );

            return { success: true, book: response.data };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    static async getBookById(id) {
        try {
            const response = await ApiClient.get(API_CONFIG.endpoints.getBook(id));
            return { success: true, book: response.data };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    static async updateBook(id, updates) {
        try {
            const formData = new FormData();
            
            if (updates.title) formData.append('Title', updates.title);
            if (updates.description) formData.append('Description', updates.description);
            if (updates.file) formData.append('File', updates.file);
            if (updates.cover) formData.append('Cover', updates.cover);
            
            if (updates.categoriesIds) {
                updates.categoriesIds.forEach(catId => {
                    formData.append('CategoriesIds', catId);
                });
            }

            const response = await ApiClient.put(
                API_CONFIG.endpoints.updateBook(id),
                formData
            );

            return { success: true, book: response.data };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    static async deleteBook(bookId) {
        try {
            await ApiClient.delete(API_CONFIG.endpoints.deleteBook(bookId));
            return { success: true };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    static async likeBook(bookId) {
        try {
            await ApiClient.post(API_CONFIG.endpoints.likeBook(bookId), {});
            return { success: true };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    static async unlikeBook(bookId) {
        try {
            await ApiClient.delete(API_CONFIG.endpoints.unlikeBook(bookId));
            return { success: true };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }
}

// ===========================================
// SERVIÇO DE COMENTÁRIOS
// ===========================================

class CommentService {
    static async getBookComments(bookId, page = 1, perPage = 10) {
        try {
            const response = await ApiClient.get(
                API_CONFIG.endpoints.getBookComments(bookId, page, perPage)
            );
            return { success: true, comments: response.data };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    static async getCommentById(commentId) {
        try {
            const response = await ApiClient.get(
                API_CONFIG.endpoints.getComment(commentId)
            );
            return { success: true, comment: response.data };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    static async createComment(bookId, text) {
        try {
            const response = await ApiClient.post(
                API_CONFIG.endpoints.createComment(bookId),
                { text }
            );
            return { success: true, comment: response.data };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    static async deleteComment(commentId) {
        try {
            await ApiClient.delete(API_CONFIG.endpoints.deleteComment(commentId));
            return { success: true };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }
}

// ===========================================
// INICIALIZAÇÃO E VERIFICAÇÕES
// ===========================================

function initializeAuth() {
    console.log('🔐 Inicializando sistema de autenticação...');

    const urlParams = new URLSearchParams(window.location.search);
    const emailToken = urlParams.get('token');
    const email = urlParams.get('email');

    if (emailToken && email) {
        handleEmailConfirmation(email, emailToken);
        return;
    }

    if (AuthService.isAuthenticated()) {
        const user = AuthService.getCurrentUser();
        console.log('✅ Usuário autenticado:', user);
        
        if (window.location.pathname.includes('login.html')) {
            window.location.href = 'index.html';
        }
        
        if (typeof currentUser !== 'undefined') {
            currentUser = {
                name: user.userName,
                email: user.email,
                id: user.userId,
                joinDate: new Date().toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' })
            };
            
            if (typeof updateAuthUI === 'function') {
                updateAuthUI();
            }
        }
    } else {
        console.log('❌ Usuário não autenticado');
    }
}

async function handleEmailConfirmation(email, token) {
    console.log('📧 Confirmando email...');
    
    if (typeof showNotification === 'function') {
        showNotification('Confirmando seu email...', 'info');
    }

    const result = await AuthService.confirmEmail(email, token);

    if (result.success) {
        if (typeof showNotification === 'function') {
            showNotification('Email confirmado com sucesso! Você já pode fazer login.', 'success');
        }
        setTimeout(() => {
            window.location.href = 'login.html';
        }, 2000);
    } else {
        if (typeof showNotification === 'function') {
            showNotification(`Erro ao confirmar email: ${result.error}`, 'error');
        }
    }
}

// ===========================================
// EXPORTAR PARA ESCOPO GLOBAL
// ===========================================

window.API_CONFIG = API_CONFIG;
window.TokenManager = TokenManager;
window.ApiClient = ApiClient;
window.AuthService = AuthService;
window.BookService = BookService;
window.CommentService = CommentService;

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeAuth);
} else {
    initializeAuth();
}

console.log('✅ API Integration carregada com paginação!');