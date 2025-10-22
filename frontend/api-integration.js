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
        login: '/login',
        loginGoogle: '/login/google',
        loginGoogleCallback: '/login/google-callback',
        register: '/users',
        confirmEmail: '/users/confirm/email',
        refreshToken: '/token/refresh-token',
        getBook: (id) => `/books/${id}`
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
    /**
     * Salva tokens no localStorage
     */
    static saveTokens(accessToken, refreshToken, refreshExpiration) {
        localStorage.setItem(API_CONFIG.storage.accessToken, accessToken);
        localStorage.setItem(API_CONFIG.storage.refreshToken, refreshToken);
        localStorage.setItem(API_CONFIG.storage.refreshExpiration, refreshExpiration);
    }

    /**
     * Obtém access token
     */
    static getAccessToken() {
        return localStorage.getItem(API_CONFIG.storage.accessToken);
    }

    /**
     * Obtém refresh token
     */
    static getRefreshToken() {
        return localStorage.getItem(API_CONFIG.storage.refreshToken);
    }

    /**
     * Verifica se refresh token expirou
     */
    static isRefreshTokenExpired() {
        const expiration = localStorage.getItem(API_CONFIG.storage.refreshExpiration);
        if (!expiration) return true;
        
        return new Date(expiration) <= new Date();
    }

    /**
     * Limpa todos os tokens
     */
    static clearTokens() {
        localStorage.removeItem(API_CONFIG.storage.accessToken);
        localStorage.removeItem(API_CONFIG.storage.refreshToken);
        localStorage.removeItem(API_CONFIG.storage.refreshExpiration);
    }

    /**
     * Salva dados do usuário
     */
    static saveUserData(userData) {
        localStorage.setItem(API_CONFIG.storage.userData, JSON.stringify(userData));
    }

    /**
     * Obtém dados do usuário
     */
    static getUserData() {
        const data = localStorage.getItem(API_CONFIG.storage.userData);
        return data ? JSON.parse(data) : null;
    }

    /**
     * Limpa dados do usuário
     */
    static clearUserData() {
        localStorage.removeItem(API_CONFIG.storage.userData);
    }
}

// ===========================================
// CLIENTE HTTP COM INTERCEPTORS
// ===========================================

class ApiClient {
    /**
     * Faz requisição HTTP com tratamento de erros e refresh token automático
     */
    static async request(endpoint, options = {}) {
        const url = API_CONFIG.baseURL + endpoint;
        
        // Adicionar token de autenticação se disponível
        const token = TokenManager.getAccessToken();
        if (token && !options.skipAuth) {
            options.headers = {
                ...options.headers,
                'Authorization': `Bearer ${token}`
            };
        }

        // Adicionar Content-Type padrão para JSON
        if (options.body && typeof options.body === 'object') {
            options.headers = {
                ...options.headers,
                'Content-Type': 'application/json'
            };
            options.body = JSON.stringify(options.body);
        }

        try {
            const response = await fetch(url, options);

            // Se 401 e temos refresh token, tentar renovar
            if (response.status === 401 && !options.skipAuth && !options.isRefreshRequest) {
                const refreshed = await this.refreshAccessToken();
                if (refreshed) {
                    // Tentar novamente com novo token
                    return this.request(endpoint, options);
                } else {
                    // Refresh falhou, fazer logout
                    this.handleUnauthorized();
                    throw new Error('Sessão expirada. Faça login novamente.');
                }
            }

            // Processar resposta
            const data = await this.handleResponse(response);
            return { success: true, data };

        } catch (error) {
            console.error('Erro na requisição:', error);
            throw error;
        }
    }

    /**
     * Processa resposta da API
     */
    static async handleResponse(response) {
        const contentType = response.headers.get('content-type');
        const isJson = contentType && contentType.includes('application/json');

        // Resposta sem conteúdo
        if (response.status === 204) {
            return null;
        }

        // Tentar parsear JSON
        let data = null;
        if (isJson) {
            data = await response.json();
        } else {
            data = await response.text();
        }

        // Verificar se resposta foi bem-sucedida
        if (!response.ok) {
            // Tratar erros da API
            const error = this.parseError(data, response.status);
            throw error;
        }

        return data;
    }

    /**
     * Parseia erros da API
     */
    static parseError(data, status) {
        // Estrutura: RequestException ou UnauthorizedException com array 'errors'
        if (data && data.errors && Array.isArray(data.errors)) {
            return new Error(data.errors.join('\n'));
        }

        if (data && data.message) {
            return new Error(data.message);
        }

        // Mensagens padrão por status code
        const defaultMessages = {
            400: 'Requisição inválida. Verifique os dados enviados.',
            401: 'Não autorizado. Faça login novamente.',
            403: 'Acesso negado.',
            404: 'Recurso não encontrado.',
            500: 'Erro interno do servidor. Tente novamente mais tarde.'
        };

        return new Error(defaultMessages[status] || 'Erro desconhecido na requisição.');
    }

    /**
     * Renova access token usando refresh token
     */
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

    /**
     * Trata caso de não autorizado (logout)
     */
    static handleUnauthorized() {
        TokenManager.clearTokens();
        TokenManager.clearUserData();
        
        // Se estiver na página principal, mostrar modal de login
        if (window.location.pathname.includes('index.html') || window.location.pathname === '/') {
            if (typeof showLoginModal === 'function') {
                showLoginModal();
            }
        }
    }

    // Métodos de conveniência
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
    /**
     * Faz login do usuário
     * @param {string} email - Email ou username
     * @param {string} password - Senha
     */
    static async login(email, password) {
        try {
            const response = await ApiClient.post(
                API_CONFIG.endpoints.login,
                { email, password },
                { skipAuth: true }
            );

            if (response.success && response.data) {
                // Salvar tokens
                TokenManager.saveTokens(
                    response.data.accessToken,
                    response.data.refreshToken,
                    response.data.refreshTokenExpiration
                );

                // Decodificar token para obter dados do usuário
                const userData = this.decodeToken(response.data.accessToken);
                TokenManager.saveUserData(userData);

                return { success: true, user: userData };
            }

            return { success: false, error: 'Resposta inválida do servidor' };

        } catch (error) {
            return { success: false, error: error.message };
        }
    }

    /**
     * Registra novo usuário
     * @param {string} userName - Nome de usuário/nick
     * @param {string} email - Email
     * @param {string} password - Senha
     */
    static async register(userName, email, password) {
        try {
            const response = await ApiClient.post(
                API_CONFIG.endpoints.register,
                { userName, email, password },
                { skipAuth: true }
            );

            if (response.success && response.data) {
                // Retornar dados do usuário criado
                // UserResponse: { id, userName, email, createdAt }
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

    /**
     * Confirma email do usuário
     * @param {string} email - Email
     * @param {string} token - Token de confirmação
     */
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

    /**
     * Inicia fluxo de login com Google
     */
    static loginWithGoogle() {
        window.location.href = API_CONFIG.baseURL + API_CONFIG.endpoints.loginGoogle;
    }

    /**
     * Faz logout do usuário
     */
    static logout() {
        TokenManager.clearTokens();
        TokenManager.clearUserData();
    }

    /**
     * Verifica se usuário está autenticado
     */
    static isAuthenticated() {
        const token = TokenManager.getAccessToken();
        const userData = TokenManager.getUserData();
        
        if (!token || !userData) {
            return false;
        }

        // Verificar se token JWT ainda é válido
        try {
            const decoded = this.decodeToken(token);
            const currentTime = Date.now() / 1000;
            
            // Token expirado
            if (decoded.exp && decoded.exp < currentTime) {
                // Tentar renovar com refresh token
                if (!TokenManager.isRefreshTokenExpired()) {
                    // Refresh token ainda válido, pode renovar na próxima requisição
                    return true;
                }
                return false;
            }

            return true;
        } catch (error) {
            return false;
        }
    }

    /**
     * Obtém dados do usuário autenticado
     */
    static getCurrentUser() {
        if (!this.isAuthenticated()) {
            return null;
        }
        return TokenManager.getUserData();
    }

    /**
     * Decodifica token JWT (apenas payload, sem validação de assinatura)
     * Nota: Validação real é feita no backend
     */
    static decodeToken(token) {
        try {
            const parts = token.split('.');
            if (parts.length !== 3) {
                throw new Error('Token JWT inválido');
            }

            const payload = parts[1];
            const decoded = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));

            // Extrair informações relevantes
            // ClaimTypes.Sid contém o userId
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
     * Busca livro por ID
     * @param {number} id - ID do livro
     */
    static async getBookById(id) {
        try {
            const response = await ApiClient.get(API_CONFIG.endpoints.getBook(id));
            return { success: true, book: response.data };
        } catch (error) {
            return { success: false, error: error.message };
        }
    }
}

// ===========================================
// INICIALIZAÇÃO E VERIFICAÇÕES
// ===========================================

/**
 * Verifica autenticação ao carregar página
 */
function initializeAuth() {
    console.log('🔐 Inicializando sistema de autenticação...');

    // Verificar se há parâmetros de confirmação de email na URL
    const urlParams = new URLSearchParams(window.location.search);
    const emailToken = urlParams.get('token');
    const email = urlParams.get('email');

    if (emailToken && email) {
        handleEmailConfirmation(email, emailToken);
        return;
    }

    // Verificar autenticação existente
    if (AuthService.isAuthenticated()) {
        const user = AuthService.getCurrentUser();
        console.log('✅ Usuário autenticado:', user);
        
        // Se estiver na página de login, redirecionar para index
        if (window.location.pathname.includes('login.html')) {
            window.location.href = 'index.html';
        }
        
        // Restaurar sessão no frontend (se variável currentUser existir)
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
        
        // Se não estiver na página de login e não for visitante, redirecionar
        const isLoginPage = window.location.pathname.includes('login.html');
        const isIndexPage = window.location.pathname.includes('index.html') || window.location.pathname === '/';
        
        if (!isLoginPage && !isIndexPage) {
            // Páginas que requerem autenticação devem redirecionar
            // window.location.href = 'login.html';
        }
    }
}

/**
 * Trata confirmação de email
 */
async function handleEmailConfirmation(email, token) {
    console.log('📧 Confirmando email...');
    
    showNotification('Confirmando seu email...', 'info');

    const result = await AuthService.confirmEmail(email, token);

    if (result.success) {
        showNotification('Email confirmado com sucesso! Você já pode fazer login.', 'success');
        setTimeout(() => {
            window.location.href = 'login.html';
        }, 2000);
    } else {
        showNotification(`Erro ao confirmar email: ${result.error}`, 'error');
    }
}

// ===========================================
// EXPORTAR PARA ESCOPO GLOBAL
// ===========================================

// Disponibilizar serviços globalmente para uso nos outros scripts
window.API_CONFIG = API_CONFIG;
window.TokenManager = TokenManager;
window.ApiClient = ApiClient;
window.AuthService = AuthService;
window.BookService = BookService;

// Inicializar quando DOM estiver carregado
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeAuth);
} else {
    initializeAuth();
}

console.log('✅ API Integration carregada!');