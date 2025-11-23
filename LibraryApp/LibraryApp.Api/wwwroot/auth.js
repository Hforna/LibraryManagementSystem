// CONTROLE DE MODAIS

//Exibe o modal de login/cadastro
function showLoginModal() {
    document.getElementById('authModal').classList.add('show');
}

//Fecha o modal de login/cadastro
function closeAuthModal() {
    document.getElementById('authModal').classList.remove('show');
    clearAuthForms();
}

//Limpa todos os formulários de autenticação
function clearAuthForms() {
    // Limpar formulário de login
    document.getElementById('loginCredential').value = '';
    document.getElementById('loginPassword').value = '';
    
    // Limpar formulário de cadastro
    document.getElementById('registerNick').value = '';
    document.getElementById('registerEmail').value = '';
    document.getElementById('registerPassword').value = '';
    document.getElementById('confirmPassword').value = '';
    
    // Limpar formulário de recuperação
    document.getElementById('forgotEmail').value = '';
}

/*
 * Alterna entre formulários de login, cadastro e recuperação
 * Tipo do formulário ('login', 'register', 'forgot')
 */
function showAuthForm(formType) {
    // Remover classes ativas de todas as abas e formulários
    document.querySelectorAll('.auth-tab').forEach(tab => tab.classList.remove('active'));
    document.querySelectorAll('.auth-form').forEach(form => form.classList.remove('active'));
    
    // Ativar formulário selecionado
    if (formType === 'login') {
        document.querySelector('.auth-tab').classList.add('active');
        document.getElementById('loginForm').classList.add('active');
    } else if (formType === 'register') {
        document.querySelectorAll('.auth-tab')[1].classList.add('active');
        document.getElementById('registerForm').classList.add('active');
    } else if (formType === 'forgot') {
        document.getElementById('forgotForm').classList.add('active');
    }
}

/*
SISTEMA DE LOGIN
Processa o login do usuário
Aceita tanto nick quanto e-mail como credencial
 */
function login() {
    const credential = document.getElementById('loginCredential').value.trim();
    const password = document.getElementById('loginPassword').value;

    // Validação básica
    if (!credential || !password) {
        alert(messages.errors.emptyFields);
        return;
    }

    // Buscar usuário no banco de dados simulado
    const foundUser = usersDatabase.find(user => 
        user.nick.toLowerCase() === credential.toLowerCase() || 
        user.email.toLowerCase() === credential.toLowerCase()
    );

    if (foundUser && foundUser.password === password) {
        // Login com usuário existente
        currentUser = createUserSession(foundUser);
        alert(`${messages.loginSuccess} Bem-vindo de volta, ${foundUser.nick}!`);
    } else {
        // Simular login para demonstração (usuários não cadastrados)
        const isEmail = validateEmail(credential);
        const displayName = isEmail ? credential.split('@')[0] : credential;
        
        const simulatedUser = {
            nick: displayName,
            email: isEmail ? credential : credential + '@email.com',
            password: password,
            joinDate: 'Janeiro 2025'
        };
        
        currentUser = createUserSession(simulatedUser);
        alert(`${messages.loginSuccess} Bem-vindo(a), ${displayName}!`);
    }

    // Finalizar login
    updateAuthUI();
    closeAuthModal();
    updateProfileData();
}

/**
 * Cria sessão do usuário com dados completos
 userData - Dados do usuário
 Objeto completo do usuário
 */
function createUserSession(userData) {
    return {
        name: userData.nick,
        email: userData.email,
        id: userData.nick + '_' + Date.now(),
        joinDate: userData.joinDate,
        bio: userData.bio || '',
        location: userData.location || ''
    };
}

// ===========================================
// SISTEMA DE CADASTRO
// ===========================================

/**
 * Processa o cadastro de novo usuário
 */
function register() {
    const nick = document.getElementById('registerNick').value.trim();
    const email = document.getElementById('registerEmail').value.trim();
    const password = document.getElementById('registerPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    // Validações
    if (!nick || !email || !password || !confirmPassword) {
        alert(messages.errors.emptyFields);
        return;
    }

    if (!validateEmail(email)) {
        alert(messages.errors.invalidEmail);
        return;
    }

    if (password !== confirmPassword) {
        alert(messages.errors.passwordMismatch);
        return;
    }

    // Verificar se nick ou email já existem
    if (isNickTaken(nick)) {
        alert(messages.errors.nickTaken);
        return;
    }

    if (isEmailTaken(email)) {
        alert(messages.errors.emailTaken);
        return;
    }

    // Criar novo usuário
    const newUser = {
        nick: nick,
        email: email,
        password: password,
        joinDate: new Date().toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' })
    };
    
    // Adicionar ao banco de dados simulado
    usersDatabase.push(newUser);
    
    // Criar sessão
    currentUser = createUserSession(newUser);

    // Finalizar cadastro
    updateAuthUI();
    closeAuthModal();
    updateProfileData();
    alert(`${messages.registerSuccess} Bem-vindo(a) à O Caminho do Saber, ${nick}!`);
}

/**
 * Verifica se nick já está em uso
 * @param {string} nick - Nick para verificar
 * @returns {boolean} True se já existe
 */
function isNickTaken(nick) {
    return usersDatabase.some(user => 
        user.nick.toLowerCase() === nick.toLowerCase()
    );
}

/**
 * Verifica se email já está cadastrado
 * @param {string} email - Email para verificar
 * @returns {boolean} True se já existe
 */
function isEmailTaken(email) {
    return usersDatabase.some(user => 
        user.email.toLowerCase() === email.toLowerCase()
    );
}

// ===========================================
// LOGIN SOCIAL (SIMULADO)
// ===========================================

/**
 * Simula login com Google
 */
function loginWithGoogle() {
    const googleUser = {
        nick: 'GoogleUser',
        email: 'usuario@gmail.com',
        password: 'google_auth',
        joinDate: new Date().toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' }),
        provider: 'google'
    };

    currentUser = createUserSession(googleUser);
    updateAuthUI();
    closeAuthModal();
    updateProfileData();
    alert('Login com Google realizado com sucesso!');
}

/**
 * Simula login com Facebook
 */
function loginWithFacebook() {
    const facebookUser = {
        nick: 'FacebookUser',
        email: 'usuario@facebook.com',
        password: 'facebook_auth',
        joinDate: new Date().toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' }),
        provider: 'facebook'
    };

    currentUser = createUserSession(facebookUser);
    updateAuthUI();
    closeAuthModal();
    updateProfileData();
    alert('Login com Facebook realizado com sucesso!');
}

/**
 * Simula cadastro com Google
 */
function registerWithGoogle() {
    loginWithGoogle(); // Mesmo processo para demonstração
}

/**
 * Simula cadastro com Facebook
 */
function registerWithFacebook() {
    loginWithFacebook(); // Mesmo processo para demonstração
}

// ===========================================
// RECUPERAÇÃO DE SENHA
// ===========================================

/**
 * Exibe formulário de recuperação de senha
 */
function showForgotPassword() {
    showAuthForm('forgot');
}

/**
 * Processa recuperação de senha
 */
function resetPassword() {
    const credential = document.getElementById('forgotEmail').value.trim();
    
    if (!credential) {
        alert('Por favor, digite seu nick ou e-mail.');
        return;
    }

    // Verificar se usuário existe
    const foundUser = usersDatabase.find(user => 
        user.nick.toLowerCase() === credential.toLowerCase() || 
        user.email.toLowerCase() === credential.toLowerCase()
    );

    if (foundUser) {
        alert(`Link de recuperação enviado para ${foundUser.email}! Verifique sua caixa de entrada.`);
    } else {
        // Para segurança, não revelar se usuário existe
        alert(`Se um usuário com "${credential}" existir, um link de recuperação foi enviado.`);
    }
    
    // Voltar ao login
    showAuthForm('login');
    document.getElementById('forgotEmail').value = '';
}

// ===========================================
// LOGOUT E CONTROLE DE SESSÃO
// ===========================================

/**
 * Faz logout do usuário
 */
function logout() {
    if (confirm(messages.logoutConfirm)) {
        currentUser = null;
        favorites.clear();
        currentlyReading.clear();
        userStats = {
            booksRead: 0,
            readingTime: 0,
            contributions: 0,
            yearlyGoal: 12
        };
        updateAuthUI();
        alert('Logout realizado com sucesso!');
    }
}

/**
 * Atualiza interface baseada no estado de autenticação
 */
function updateAuthUI() {
    const loginBtn = document.getElementById('loginBtn');
    const userInfo = document.getElementById('userInfo');
    const userName = document.getElementById('userName');
    const profileTab = document.getElementById('profileTab');

    if (currentUser) {
        // Usuário logado
        loginBtn.classList.add('hidden');
        userInfo.classList.remove('hidden');
        profileTab.style.display = 'block';
        userName.textContent = `👋 Olá, ${currentUser.name}!`;
    } else {
        // Usuário não logado
        loginBtn.classList.remove('hidden');
        userInfo.classList.add('hidden');
        profileTab.style.display = 'none';
    }
}

/**
 * Verifica estado de autenticação na inicialização
 */
function checkAuthState() {
    // Em um sistema real, verificaria cookies/tokens aqui
    // Para demonstração, mantém estado vazio
    updateAuthUI();
}

// ===========================================
// GERENCIAMENTO DE PERFIL
// ===========================================

/**
 * Exibe modal de edição de perfil
 */
function showEditProfile() {
    if (!currentUser) return;

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
    const nick = document.getElementById('editName').value.trim();
    const email = document.getElementById('editEmail').value.trim();
    const bio = document.getElementById('editBio').value.trim();
    const location = document.getElementById('editLocation').value.trim();
    const currentPass = document.getElementById('currentPassword').value;
    const newPass = document.getElementById('newPassword').value;
    const confirmPass = document.getElementById('confirmNewPassword').value;

    // Validações básicas
    if (!nick || !email) {
        alert('Apelido e e-mail são obrigatórios.');
        return;
    }

    if (!validateEmail(email)) {
        alert(messages.errors.invalidEmail);
        return;
    }

    // Validar alteração de senha se fornecida
    if (newPass) {
        if (newPass !== confirmPass) {
            alert(messages.errors.passwordMismatch);
            return;
        }
        
        if (!currentPass) {
            alert('Digite a senha atual para alterá-la.');
            return;
        }
    }

    // Atualizar dados do usuário
    currentUser.name = nick;
    currentUser.email = email;
    currentUser.bio = bio;
    currentUser.location = location;

    // Atualizar no banco de dados se existir
    const userIndex = usersDatabase.findIndex(user => 
        user.nick === currentUser.name || user.email === currentUser.email
    );
    
    if (userIndex !== -1) {
        usersDatabase[userIndex].nick = nick;
        usersDatabase[userIndex].email = email;
        if (newPass) {
            usersDatabase[userIndex].password = newPass;
        }
    }

    // Finalizar atualização
    updateAuthUI();
    updateProfileData();
    closeEditProfile();
    alert(messages.profileUpdated);
}

// ===========================================
// VALIDAÇÃO DE RECURSOS PROTEGIDOS
// ===========================================

/**
 * Verifica se usuário está logado para acessar recurso
 * @param {string} action - Nome da ação sendo executada
 * @returns {boolean} True se autorizado
 */
function requireLogin(action = 'acessar este recurso') {
    if (!currentUser) {
        alert(`Por favor, faça login para ${action}.`);
        showLoginModal();
        return false;
    }
    return true;
}