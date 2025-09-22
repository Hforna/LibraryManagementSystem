/*
 * BIBLIOTECA VIRTUAL - AUTENTICAÇÃO DA TELA PRINCIPAL
 * Sistema simplificado de autenticação para a página principal
 * Inclui: verificação de sessão, logout, proteção de recursos
 */

// ===========================================
// VERIFICAÇÃO DE SESSÃO
// ===========================================

/**
 * Verifica se existe sessão ativa do usuário
 * Em um sistema real, verificaria localStorage/cookies
 */
function checkAuthState() {
    // Simular verificação de sessão
    // Em produção, verificaria localStorage ou cookie
    const sessionData = getStoredSession();
    
    if (sessionData && sessionData.user) {
        // Restaurar dados do usuário
        currentUser = sessionData.user;
        
        // Restaurar estatísticas se existirem
        if (sessionData.stats) {
            userStats = sessionData.stats;
        }
        
        // Restaurar favoritos e leituras
        if (sessionData.favorites) {
            sessionData.favorites.forEach(id => favorites.add(id));
        }
        
        if (sessionData.reading) {
            sessionData.reading.forEach(id => currentlyReading.add(id));
        }
        
        updateAuthUI();
        console.log('✅ Sessão restaurada:', currentUser.name);
    } else {
        // Verificar se veio do login (parâmetros da URL)
        checkLoginRedirect();
    }
}

/**
 * Verifica se veio de redirecionamento do login
 */
function checkLoginRedirect() {
    const urlParams = new URLSearchParams(window.location.search);
    const loginSuccess = urlParams.get('login');
    const userName = urlParams.get('user');
    
    if (loginSuccess === 'success' && userName) {
        // Simular dados de usuário logado
        currentUser = {
            name: decodeURIComponent(userName),
            email: userName.includes('@') ? userName : userName + '@email.com',
            id: Date.now(),
            joinDate: 'Janeiro 2025',
            bio: '',
            location: ''
        };
        
        updateAuthUI();
        updateProfileData();
        
        // Limpar parâmetros da URL
        window.history.replaceState({}, document.title, window.location.pathname);
        
        showWelcomeMessage(currentUser.name);
    }
}

/**
 * Simula recuperação de sessão armazenada
 * Em produção, usaria localStorage ou cookies
 */
function getStoredSession() {
    // Simular dados de sessão
    // Em produção: return JSON.parse(localStorage.getItem('userSession'));
    return null;
}

/**
 * Exibe mensagem de boas-vindas
 * @param {string} userName - Nome do usuário
 */
function showWelcomeMessage(userName) {
    showNotification(`Bem-vindo de volta, ${userName}!`, 'success');
    
    // Destacar recursos disponíveis para usuário logado
    setTimeout(() => {
        highlightUserFeatures();
    }, 2000);
}

/**
 * Destaca recursos disponíveis para usuários logados
 */
function highlightUserFeatures() {
    const userFeatures = [
        { tab: 'favorites', message: 'Agora você pode favoritar livros!' },
        { tab: 'upload', message: 'Contribua enviando seus próprios livros!' },
        { tab: 'profile', message: 'Confira seu perfil personalizado!' }
    ];
    
    userFeatures.forEach((feature, index) => {
        setTimeout(() => {
            const tab = document.querySelector(`[onclick="showTab('${feature.tab}')"]`);
            if (tab) {
                tab.style.animation = 'pulse 1s ease-in-out';
                showNotification(feature.message, 'info');
            }
        }, index * 3000);
    });
}

// ===========================================
// CONTROLE DE INTERFACE AUTENTICADA
// ===========================================

/**
 * Atualiza interface baseada no estado de autenticação
 */
function updateAuthUI() {
    const loginBtn = document.getElementById('loginBtn');
    const userInfo = document.getElementById('userInfo');
    const userName = document.getElementById('userName');
    const profileTab = document.getElementById('profileTab');

    if (currentUser) {
        // Usuário logado - mostrar informações do usuário
        if (loginBtn) loginBtn.style.display = 'none';
        if (userInfo) userInfo.classList.remove('hidden');
        if (profileTab) profileTab.style.display = 'block';
        if (userName) userName.textContent = `👋 Olá, ${currentUser.name}!`;
        
        // Mostrar formulários que requerem login
        showAuthenticatedContent();
        
    } else {
        // Usuário não logado - mostrar botão de login
        if (loginBtn) loginBtn.style.display = 'block';
        if (userInfo) userInfo.classList.add('hidden');
        if (profileTab) profileTab.style.display = 'none';
        
        // Esconder conteúdo que requer login
        hideAuthenticatedContent();
    }
}

/**
 * Mostra conteúdo que requer autenticação
 */
function showAuthenticatedContent() {
    // Mostrar formulário de upload
    const uploadForm = document.getElementById('uploadForm');
    const uploadLoginRequired = document.getElementById('uploadLoginRequired');
    if (uploadForm) uploadForm.style.display = 'block';
    if (uploadLoginRequired) uploadLoginRequired.style.display = 'none';
    
    // Mostrar favoritos normalmente
    const favoritesLoginRequired = document.getElementById('favoritesLoginRequired');
    if (favoritesLoginRequired) favoritesLoginRequired.style.display = 'none';
}

/**
 * Esconde conteúdo que requer autenticação
 */
function hideAuthenticatedContent() {
    // Esconder formulário de upload e mostrar prompt de login
    const uploadForm = document.getElementById('uploadForm');
    const uploadLoginRequired = document.getElementById('uploadLoginRequired');
    if (uploadForm) uploadForm.style.display = 'none';
    if (uploadLoginRequired) uploadLoginRequired.style.display = 'block';
    
    // Mostrar prompt de login nos favoritos se não houver nenhum
    const favoritesLoginRequired = document.getElementById('favoritesLoginRequired');
    if (favoritesLoginRequired && favorites.size === 0) {
        favoritesLoginRequired.style.display = 'block';
    }
}

// ===========================================
// LOGOUT E GERENCIAMENTO DE SESSÃO
// ===========================================

/**
 * Faz logout do usuário atual
 */
function logout() {
    if (!currentUser) return;
    
    const userName = currentUser.name;
    
    if (confirm(`Tem certeza que deseja sair, ${userName}?`)) {
        // Simular salvamento antes do logout
        saveUserSession();
        
        // Limpar dados do usuário
        currentUser = null;
        favorites.clear();
        currentlyReading.clear();
        userStats = {
            booksRead: 0,
            readingTime: 0,
            contributions: 0,
            yearlyGoal: 12
        };
        
        // Atualizar interface
        updateAuthUI();
        
        // Voltar para aba inicial
        showTab('home');
        
        // Mostrar mensagem de despedida
        showNotification(`Até logo, ${userName}!`, 'info');
        
        // Limpar sessão armazenada
        clearStoredSession();
    }
}

/**
 * Salva dados da sessão atual
 * Em produção, salvaria no localStorage
 */
function saveUserSession() {
    if (!currentUser) return;
    
    const sessionData = {
        user: currentUser,
        stats: userStats,
        favorites: Array.from(favorites),
        reading: Array.from(currentlyReading),
        timestamp: new Date().toISOString()
    };
    
    // Em produção: localStorage.setItem('userSession', JSON.stringify(sessionData));
    console.log('💾 Sessão salva:', sessionData);
}

/**
 * Limpa sessão armazenada
 */
function clearStoredSession() {
    // Em produção: localStorage.removeItem('userSession');
    console.log('🗑️ Sessão limpa');
}

// ===========================================
// PROTEÇÃO DE RECURSOS
// ===========================================

/**
 * Verifica se usuário está logado para acessar recurso
 * @param {string} action - Nome da ação sendo executada
 * @returns {boolean} True se autorizado
 */
function requireLogin(action = 'acessar este recurso') {
    if (!currentUser) {
        showLoginPrompt(action);
        return false;
    }
    return true;
}

/**
 * Exibe prompt para fazer login
 * @param {string} action - Ação que requer login
 */
function showLoginPrompt(action) {
    const message = `Para ${action}, você precisa estar logado.`;
    showNotification(message, 'warning');
    
    // Criar prompt modal customizado
    setTimeout(() => {
        if (confirm(`${message}\n\nGostaria de fazer login agora?`)) {
            window.location.href = 'login.html';
        }
    }, 500);
}

// ===========================================
// INTEGRAÇÃO COM FUNCIONALIDADES
// ===========================================

/**
 * Override da função toggleFavorite para verificar login
 * @param {number} bookId - ID do livro
 */
function toggleFavorite(bookId) {
    if (!requireLogin('favoritar livros')) {
        return;
    }

    const book = books.find(b => b.id === bookId);
    if (!book) return;

    if (favorites.has(bookId)) {
        favorites.delete(bookId);
        showNotification(`"${book.title}" removido dos favoritos!`, 'info');
    } else {
        favorites.add(bookId);
        showNotification(`"${book.title}" adicionado aos favoritos!`, 'success');
    }
    
    // Re-renderizar para atualizar botões
    const currentTab = document.querySelector('.tab-content.active');
    if (currentTab && currentTab.id === 'home') {
        renderBooks(books);
    } else if (currentTab && currentTab.id === 'favorites') {
        renderFavorites();
    }
    
    updateProfileData();
    saveUserSession();
}

/**
 * Override da função uploadBook para verificar login
 */
function uploadBook() {
    if (!requireLogin('contribuir com livros')) {
        return;
    }

    const title = document.getElementById('bookTitle').value.trim();
    const author = document.getElementById('bookAuthor').value.trim();
    const category = document.getElementById('bookCategory').value;
    const description = document.getElementById('bookDescription').value.trim();
    const file = document.getElementById('bookFile').files[0];

    // Validações
    if (!title || !author || !category || !description) {
        showNotification('Por favor, preencha todos os campos obrigatórios.', 'error');
        return;
    }

    if (file && file.size > appConfig.maxFileSize) {
        showNotification('Arquivo muito grande. Tamanho máximo: 50MB.', 'error');
        return;
    }

    if (file && !isValidFileFormat(file.name)) {
        showNotification('Formato não suportado. Use PDF, EPUB ou TXT.', 'error');
        return;
    }

    // Criar novo livro
    const newBook = {
        id: generateUniqueId(),
        title: title,
        author: author,
        category: category,
        icon: getCategoryIcon(category),
        description: description,
        content: generateSampleContent(title, author, description),
        uploadedBy: currentUser.name,
        uploadDate: new Date().toLocaleDateString('pt-BR')
    };

    books.push(newBook);
    userStats.contributions++;
    updateProfileData();
    
    // Limpar formulário
    clearUploadForm();
    
    // Atualizar estatísticas
    updateStats();
    
    // Salvar sessão
    saveUserSession();
    
    showNotification(`Livro "${title}" enviado com sucesso! Obrigado, ${currentUser.name}!`, 'success');
}

// ===========================================
// FUNÇÕES AUXILIARES
// ===========================================

/**
 * Verifica se formato do arquivo é válido
 * @param {string} filename - Nome do arquivo
 * @returns {boolean} True se válido
 */
function isValidFileFormat(filename) {
    const extension = '.' + filename.split('.').pop().toLowerCase();
    return appConfig.supportedFormats.includes(extension);
}

/**
 * Limpa formulário de upload
 */
function clearUploadForm() {
    document.getElementById('bookTitle').value = '';
    document.getElementById('bookAuthor').value = '';
    document.getElementById('bookCategory').value = '';
    document.getElementById('bookDescription').value = '';
    document.getElementById('bookFile').value = '';
}

/**
 * Gera conteúdo de exemplo para livro enviado
 * @param {string} title - Título do livro
 * @param {string} author - Autor do livro
 * @param {string} description - Descrição do livro
 * @returns {string} Conteúdo HTML
 */
function generateSampleContent(title, author, description) {
    return `
        <h3>Prefácio</h3>
        <p>Este é o conteúdo do livro "<strong>${title}</strong>" por <em>${author}</em>.</p>
        
        <h4>Sobre a Obra:</h4>
        <p>${description}</p>
        
        <h3>Capítulo 1</h3>
        <p>Conteúdo do primeiro capítulo será exibido aqui após o processamento do arquivo enviado.</p>
        
        <p><em>Nota: Este é um conteúdo de exemplo. Em um sistema real, o conteúdo seria extraído do arquivo enviado pelo usuário <strong>${author}</strong>.</em></p>
        
        <h4>Informações de Contribuição:</h4>
        <ul>
            <li><strong>Contribuído por:</strong> ${currentUser ? currentUser.name : 'Usuário'}</li>
            <li><strong>Data de envio:</strong> ${new Date().toLocaleDateString('pt-BR')}</li>
            <li><strong>Categoria:</strong> ${getCategoryName(title)}</li>
        </ul>
    `;
}

/**
 * Retorna nome da categoria baseado no título (função auxiliar)
 * @param {string} title - Título do livro
 * @returns {string} Nome da categoria
 */
function getCategoryName(title) {
    // Lógica simples para categorizar baseado em palavras-chave
    const titleLower = title.toLowerCase();
    
    if (titleLower.includes('história') || titleLower.includes('histórico')) return 'História';
    if (titleLower.includes('ciência') || titleLower.includes('física') || titleLower.includes('química')) return 'Ciência';
    if (titleLower.includes('filosofia') || titleLower.includes('ética')) return 'Filosofia';
    if (titleLower.includes('tecnologia') || titleLower.includes('programação') || titleLower.includes('javascript')) return 'Tecnologia';
    if (titleLower.includes('autoajuda') || titleLower.includes('motivação')) return 'Autoajuda';
    
    return 'Literatura'; // Categoria padrão
}

// ===========================================
// GERENCIAMENTO DE PERFIL
// ===========================================

/**
 * Exibe modal de edição de perfil
 */
function showEditProfile() {
    if (!currentUser) {
        requireLogin('editar perfil');
        return;
    }

    // Preencher formulário com dados atuais
    document.getElementById('editName').value = currentUser.name;
    document.getElementById('editEmail').value = currentUser.email;
    document.getElementById('editBio').value = currentUser.bio || '';
    document.getElementById('editLocation').value = currentUser.location || '';

    document.getElementById('editProfileModal').classList.add('show');
}

/**
 * Fecha modal de edição de perfil
 */
function closeEditProfile() {
    document.getElementById('editProfileModal').classList.remove('show');
    
    // Limpar campos de senha por segurança
    document.getElementById('currentPassword').value = '';
    document.getElementById('newPassword').value = '';
    document.getElementById('confirmNewPassword').value = '';
}

/**
 * Atualiza dados do perfil do usuário
 */
function updateProfile() {
    if (!currentUser) return;

    const nick = document.getElementById('editName').value.trim();
    const email = document.getElementById('editEmail').value.trim();
    const bio = document.getElementById('editBio').value.trim();
    const location = document.getElementById('editLocation').value.trim();
    const currentPass = document.getElementById('currentPassword').value;
    const newPass = document.getElementById('newPassword').value;
    const confirmPass = document.getElementById('confirmNewPassword').value;

    // Validações básicas
    if (!nick || !email) {
        showNotification('Apelido e e-mail são obrigatórios.', 'error');
        return;
    }

    if (!validateEmail(email)) {
        showNotification('Por favor, digite um e-mail válido.', 'error');
        return;
    }

    // Validar alteração de senha se fornecida
    if (newPass) {
        if (newPass !== confirmPass) {
            showNotification('As novas senhas não coincidem.', 'error');
            return;
        }
    }

    // Atualizar dados do usuário
    currentUser.name = nick;
    currentUser.email = email;
    currentUser.bio = bio;
    currentUser.location = location;

    // Salvar alterações
    saveUserSession();
    updateAuthUI();
    updateProfileData();
    closeEditProfile();
    
    showNotification('Perfil atualizado com sucesso!', 'success');
}

// ===========================================
// AUTO-SAVE E PERSISTÊNCIA
// ===========================================

/**
 * Configura auto-save automático dos dados do usuário
 */
function setupAutoSave() {
    if (!currentUser) return;
    
    // Auto-save a cada minuto se houver usuário logado
    setInterval(() => {
        if (currentUser) {
            saveUserSession();
            console.log('💾 Auto-save executado para:', currentUser.name);
        }
    }, appConfig.autoSaveInterval);
}

/**
 * Salva progresso antes de sair da página
 */
function setupBeforeUnload() {
    window.addEventListener('beforeunload', function(e) {
        if (currentUser) {
            saveUserSession();
        }
    });
}

// ===========================================
// INICIALIZAÇÃO
// ===========================================

/**
 * Inicializa sistema de autenticação da tela principal
 */
function initMainAuth() {
    console.log('🔐 Inicializando autenticação da tela principal...');
    
    // Verificar estado de autenticação
    checkAuthState();
    
    // Configurar auto-save
    setupAutoSave();
    
    // Configurar salvamento antes de sair
    setupBeforeUnload();
    
    console.log('✅ Sistema de autenticação inicializado');
}

// Chamar inicialização quando DOM estiver carregado
document.addEventListener('DOMContentLoaded', function() {
    // Aguardar um pouco para garantir que outros scripts carregaram
    setTimeout(initMainAuth, 100);
});
        
        
        if (!currentPass) {
            showNotification('Digite a senha atual para alterá-la.', 'error');
            return;
        }