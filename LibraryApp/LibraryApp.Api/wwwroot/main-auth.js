// Verificação de sessação
function checkAuthState() {
    console.log('🔍 Verificando estado de autenticação...');
    
    if (AuthService && AuthService.isAuthenticated()) {
        const userData = AuthService.getCurrentUser();
        
        if (userData) {
            // Restaurar dados do usuário na aplicação
            currentUser = {
                name: userData.userName || userData.name,
                email: userData.email,
                id: userData.userId,
                joinDate: new Date().toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' }),
                bio: '',
                location: ''
            };
            
            updateAuthUI();
            updateProfileData();
            console.log('✅ Sessão restaurada:', currentUser.name);
            
            // Mostrar mensagem de boas-vindas
            showWelcomeMessage(currentUser.name);
        }
    } else {
        console.log('❌ Usuário não autenticado');
        // Limpar dados de usuário se existir
        if (typeof currentUser !== 'undefined') {
            currentUser = null;
        }
        updateAuthUI();
    }
}

function showWelcomeMessage(userName) {
    // Mostrar apenas na primeira vez (verificar sessionStorage temporário)
    const alreadyWelcomed = sessionStorage.getItem('welcomed');
    if (!alreadyWelcomed) {
        setTimeout(() => {
            showNotification(`Bem-vindo de volta, ${userName}!`, 'success');
        }, 500);
        sessionStorage.setItem('welcomed', 'true');
    }
}

// Controle de interface autenticada
function updateAuthUI() {
    const loginBtn = document.getElementById('loginBtn');
    const userInfo = document.getElementById('userInfo');
    const userName = document.getElementById('userName');
    const profileTab = document.getElementById('profileTab');

    if (currentUser) {
        // Usuário logado
        if (loginBtn) loginBtn.style.display = 'none';
        if (userInfo) userInfo.classList.remove('hidden');
        if (profileTab) profileTab.style.display = 'block';
        if (userName) userName.textContent = `👋 Olá, ${currentUser.name}!`;
        
        showAuthenticatedContent();
    } else {
        // Usuário não logado
        if (loginBtn) loginBtn.style.display = 'block';
        if (userInfo) userInfo.classList.add('hidden');
        if (profileTab) profileTab.style.display = 'none';
        
        hideAuthenticatedContent();
    }
}

function showAuthenticatedContent() {
    const uploadForm = document.getElementById('uploadForm');
    const uploadLoginRequired = document.getElementById('uploadLoginRequired');
    if (uploadForm) uploadForm.style.display = 'block';
    if (uploadLoginRequired) uploadLoginRequired.style.display = 'none';
    
    const favoritesLoginRequired = document.getElementById('favoritesLoginRequired');
    if (favoritesLoginRequired) favoritesLoginRequired.style.display = 'none';
}

function hideAuthenticatedContent() {
    const uploadForm = document.getElementById('uploadForm');
    const uploadLoginRequired = document.getElementById('uploadLoginRequired');
    if (uploadForm) uploadForm.style.display = 'none';
    if (uploadLoginRequired) uploadLoginRequired.style.display = 'block';
    
    const favoritesLoginRequired = document.getElementById('favoritesLoginRequired');
    if (favoritesLoginRequired && favorites.size === 0) {
        favoritesLoginRequired.style.display = 'block';
    }
}

// Logout e gerenciamento de sessação
function logout() {
    if (!currentUser) return;
    
    const userName = currentUser.name;
    
    if (confirm(`Tem certeza que deseja sair, ${userName}?`)) {
        // Usar AuthService para fazer logout
        if (AuthService) {
            AuthService.logout();
        }
        
        // Limpar dados locais
        currentUser = null;
        favorites.clear();
        currentlyReading.clear();
        userStats = {
            booksRead: 0,
            readingTime: 0,
            contributions: 0,
            yearlyGoal: 12
        };
        
        // Limpar sessionStorage temporário
        sessionStorage.removeItem('welcomed');
        
        updateAuthUI();
        showTab('home');
        showNotification(`Até logo, ${userName}!`, 'info');
    }
}

// Proteção de recursos
function requireLogin(action = 'acessar este recurso') {
    if (!currentUser) {
        showLoginPrompt(action);
        return false;
    }
    return true;
}

function showLoginPrompt(action) {
    const message = `Para ${action}, você precisa estar logado.`;
    showNotification(message, 'warning');
    
    setTimeout(() => {
        if (confirm(`${message}\n\nGostaria de fazer login agora?`)) {
            window.location.href = 'login.html';
        }
    }, 500);
}

// Integração com funcionalidades
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
    
    // Re-renderizar
    const currentTab = document.querySelector('.tab-content.active');
    if (currentTab && currentTab.id === 'home') {
        renderBooks(books);
    } else if (currentTab && currentTab.id === 'favorites') {
        renderFavorites();
    }
    
    updateProfileData();
    saveUserSession();
}

function uploadBook() {
    if (!requireLogin('contribuir com livros')) {
        return;
    }

    const title = document.getElementById('bookTitle').value.trim();
    const author = document.getElementById('bookAuthor').value.trim();
    const category = document.getElementById('bookCategory').value;
    const description = document.getElementById('bookDescription').value.trim();
    const file = document.getElementById('bookFile').files[0];

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
    clearUploadForm();
    updateStats();
    saveUserSession();
    
    showNotification(`Livro "${title}" enviado com sucesso! Obrigado, ${currentUser.name}!`, 'success');
}

// Funções auxiliares
function isValidFileFormat(filename) {
    const extension = '.' + filename.split('.').pop().toLowerCase();
    return appConfig.supportedFormats.includes(extension);
}

function clearUploadForm() {
    document.getElementById('bookTitle').value = '';
    document.getElementById('bookAuthor').value = '';
    document.getElementById('bookCategory').value = '';
    document.getElementById('bookDescription').value = '';
    document.getElementById('bookFile').value = '';
}

function generateSampleContent(title, author, description) {
    return `
        <h3>Prefácio</h3>
        <p>Este é o conteúdo do livro "<strong>${title}</strong>" por <em>${author}</em>.</p>
        
        <h4>Sobre a Obra:</h4>
        <p>${description}</p>
        
        <h3>Capítulo 1</h3>
        <p>Conteúdo do primeiro capítulo será exibido aqui após o processamento do arquivo enviado.</p>
        
        <p><em>Nota: Este é um conteúdo de exemplo. O conteúdo real será processado do arquivo enviado.</em></p>
        
        <h4>Informações de Contribuição:</h4>
        <ul>
            <li><strong>Contribuído por:</strong> ${currentUser ? currentUser.name : 'Usuário'}</li>
            <li><strong>Data de envio:</strong> ${new Date().toLocaleDateString('pt-BR')}</li>
            <li><strong>Categoria:</strong> ${getCategoryName(title)}</li>
        </ul>
    `;
}

function getCategoryName(title) {
    const titleLower = title.toLowerCase();
    
    if (titleLower.includes('história') || titleLower.includes('histórico')) return 'História';
    if (titleLower.includes('ciência') || titleLower.includes('física') || titleLower.includes('química')) return 'Ciência';
    if (titleLower.includes('filosofia') || titleLower.includes('ética')) return 'Filosofia';
    if (titleLower.includes('tecnologia') || titleLower.includes('programação')) return 'Tecnologia';
    if (titleLower.includes('autoajuda') || titleLower.includes('motivação')) return 'Autoajuda';
    
    return 'Literatura';
}

// Gerenciamento de perfil
function showEditProfile() {
    if (!currentUser) {
        requireLogin('editar perfil');
        return;
    }

    document.getElementById('editName').value = currentUser.name;
    document.getElementById('editEmail').value = currentUser.email;
    document.getElementById('editBio').value = currentUser.bio || '';
    document.getElementById('editLocation').value = currentUser.location || '';

    document.getElementById('editProfileModal').classList.add('show');
}

function closeEditProfile() {
    document.getElementById('editProfileModal').classList.remove('show');
    
    document.getElementById('currentPassword').value = '';
    document.getElementById('newPassword').value = '';
    document.getElementById('confirmNewPassword').value = '';
}

function updateProfile() {
    if (!currentUser) return;

    const nick = document.getElementById('editName').value.trim();
    const email = document.getElementById('editEmail').value.trim();
    const bio = document.getElementById('editBio').value.trim();
    const location = document.getElementById('editLocation').value.trim();
    const currentPass = document.getElementById('currentPassword').value;
    const newPass = document.getElementById('newPassword').value;
    const confirmPass = document.getElementById('confirmNewPassword').value;

    if (!nick || !email) {
        showNotification('Apelido e e-mail são obrigatórios.', 'error');
        return;
    }

    if (!validateEmail(email)) {
        showNotification('Por favor, digite um e-mail válido.', 'error');
        return;
    }

    if (newPass) {
        if (newPass !== confirmPass) {
            showNotification('As novas senhas não coincidem.', 'error');
            return;
        }
        
        if (!currentPass) {
            showNotification('Digite a senha atual para alterá-la.', 'error');
            return;
        }
    }

    // Atualizar dados locais
    currentUser.name = nick;
    currentUser.email = email;
    currentUser.bio = bio;
    currentUser.location = location;

    saveUserSession();
    updateAuthUI();
    updateProfileData();
    closeEditProfile();
    
    showNotification('Perfil atualizado com sucesso!', 'success');
}

// Auto-Save e persistência
function saveUserSession() {
    // Dados já são salvos pelo TokenManager
    console.log('💾 Sessão salva (via TokenManager)');
}

function setupAutoSave() {
    if (!currentUser) return;
    
    setInterval(() => {
        if (currentUser) {
            saveUserSession();
            console.log('💾 Auto-save executado');
        }
    }, appConfig.autoSaveInterval);
}

function setupBeforeUnload() {
    window.addEventListener('beforeunload', function(e) {
        if (currentUser) {
            saveUserSession();
        }
    });
}

// Inicialização
function initMainAuth() {
    console.log('🔐 Inicializando autenticação da tela principal...');
    
    // Aguardar AuthService estar disponível
    if (typeof AuthService === 'undefined') {
        console.log('⏳ Aguardando AuthService...');
        setTimeout(initMainAuth, 100);
        return;
    }
    
    checkAuthState();
    setupAutoSave();
    setupBeforeUnload();
    
    console.log('✅ Sistema de autenticação inicializado');
}

// Chamar inicialização quando DOM estiver carregado
document.addEventListener('DOMContentLoaded', function() {
    setTimeout(initMainAuth, 100);
});