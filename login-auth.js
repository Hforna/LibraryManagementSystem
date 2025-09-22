/*
 * BIBLIOTECA VIRTUAL - JAVASCRIPT DA TELA DE LOGIN
 * Lógica de autenticação e validações para a tela independente
 * Inclui: login, cadastro, validações em tempo real, animações
 */

// ===========================================
// CONTROLE DE FORMULÁRIOS
// ===========================================

/**
 * Alterna entre formulários de login, cadastro e recuperação
 * @param {string} formType - Tipo do formulário ('login', 'register', 'forgot')
 */
function showAuthForm(formType) {
    // Remover classes ativas
    document.querySelectorAll('.auth-tab').forEach(tab => tab.classList.remove('active'));
    document.querySelectorAll('.auth-form').forEach(form => form.classList.remove('active'));
    
    // Ativar formulário selecionado
    if (formType === 'login') {
        document.querySelectorAll('.auth-tab')[0].classList.add('active');
        document.getElementById('loginForm').classList.add('active');
    } else if (formType === 'register') {
        document.querySelectorAll('.auth-tab')[1].classList.add('active');
        document.getElementById('registerForm').classList.add('active');
    } else if (formType === 'forgot') {
        document.getElementById('forgotForm').classList.add('active');
    }
}

/**
 * Exibe formulário de recuperação de senha
 */
function showForgotPassword() {
    showAuthForm('forgot');
}

// ===========================================
// SISTEMA DE LOGIN
// ===========================================

/**
 * Processa o login do usuário
 * @param {Event} event - Evento do formulário
 */
function login(event) {
    event.preventDefault();
    
    const credential = document.getElementById('loginCredential').value.trim();
    const password = document.getElementById('loginPassword').value;

    // Validação básica
    if (!credential || !password) {
        showNotification('Por favor, preencha todos os campos.', 'error');
        return;
    }

    // Mostrar loading
    showButtonLoading('loginForm');

    // Simular delay de autenticação
    setTimeout(() => {
        // Buscar usuário no banco de dados simulado
        const foundUser = usersDatabase.find(user => 
            user.nick.toLowerCase() === credential.toLowerCase() || 
            user.email.toLowerCase() === credential.toLowerCase()
        );

        if (foundUser && foundUser.password === password) {
            // Login bem-sucedido
            saveUserSession(foundUser);
            showNotification(`Bem-vindo de volta, ${foundUser.nick}!`, 'success');
            
            // Redirecionar após 1.5 segundos
            setTimeout(() => {
                window.location.href = 'index.html';
            }, 1500);
            
        } else {
            // Verificar se é um login de demonstração
            if (tryDemoLogin(credential, password)) {
                return;
            }
            
            // Login falhou
            hideButtonLoading('loginForm');
            showNotification('Credenciais inválidas. Tente novamente.', 'error');
            
            // Destacar campos com erro
            addValidationClass('loginCredential', 'error');
            addValidationClass('loginPassword', 'error');
        }
    }, 1000);
}

/**
 * Tenta login com credenciais de demonstração
 * @param {string} credential - Credencial informada
 * @param {string} password - Senha informada
 * @returns {boolean} True se foi login de demo
 */
function tryDemoLogin(credential, password) {
    // Permitir login simulado para demonstração
    const isEmail = validateEmail(credential);
    const displayName = isEmail ? credential.split('@')[0] : credential;
    
    const simulatedUser = {
        nick: displayName,
        email: isEmail ? credential : credential + '@email.com',
        password: password,
        joinDate: 'Janeiro 2025'
    };
    
    saveUserSession(simulatedUser);
    showNotification(`Bem-vindo, ${displayName}! (Login de demonstração)`, 'success');
    
    setTimeout(() => {
        window.location.href = 'index.html';
    }, 1500);
    
    return true;
}

// ===========================================
// SISTEMA DE CADASTRO
// ===========================================

/**
 * Processa o cadastro de novo usuário
 * @param {Event} event - Evento do formulário
 */
function register(event) {
    event.preventDefault();
    
    const nick = document.getElementById('registerNick').value.trim();
    const email = document.getElementById('registerEmail').value.trim();
    const password = document.getElementById('registerPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    const acceptTerms = document.getElementById('acceptTerms').checked;

    // Validações
    if (!nick || !email || !password || !confirmPassword) {
        showNotification('Por favor, preencha todos os campos obrigatórios.', 'error');
        return;
    }

    if (!validateEmail(email)) {
        showNotification('Por favor, digite um e-mail válido.', 'error');
        addValidationClass('registerEmail', 'error');
        return;
    }

    if (password !== confirmPassword) {
        showNotification('As senhas não coincidem.', 'error');
        addValidationClass('confirmPassword', 'error');
        return;
    }

    if (getPasswordStrength(password) === 'weak') {
        showNotification('Senha muito fraca. Use pelo menos 6 caracteres.', 'error');
        addValidationClass('registerPassword', 'error');
        return;
    }

    if (!acceptTerms) {
        showNotification('Você deve aceitar os termos de uso.', 'error');
        return;
    }

    // Verificar duplicatas
    if (isNickTaken(nick)) {
        showNotification('Este nick já está em uso. Escolha outro.', 'error');
        addValidationClass('registerNick', 'error');
        return;
    }

    if (isEmailTaken(email)) {
        showNotification('Este e-mail já está cadastrado.', 'error');
        addValidationClass('registerEmail', 'error');
        return;
    }

    // Mostrar loading
    showButtonLoading('registerForm');

    // Simular delay de cadastro
    setTimeout(() => {
        // Criar novo usuário
        const newUser = {
            nick: nick,
            email: email,
            password: password,
            joinDate: new Date().toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' })
        };
        
        // Adicionar ao banco simulado
        usersDatabase.push(newUser);
        
        // Salvar sessão
        saveUserSession(newUser);
        
        showNotification(`Conta criada com sucesso! Bem-vindo, ${nick}!`, 'success');
        
        // Redirecionar
        setTimeout(() => {
            window.location.href = 'index.html';
        }, 1500);
        
    }, 1200);
}

// ===========================================
// RECUPERAÇÃO DE SENHA
// ===========================================

/**
 * Processa recuperação de senha
 * @param {Event} event - Evento do formulário
 */
function resetPassword(event) {
    event.preventDefault();
    
    const credential = document.getElementById('forgotCredential').value.trim();
    
    if (!credential) {
        showNotification('Por favor, digite seu nick ou e-mail.', 'error');
        return;
    }

    // Mostrar loading
    showButtonLoading('forgotForm');

    // Simular envio de e-mail
    setTimeout(() => {
        const foundUser = usersDatabase.find(user => 
            user.nick.toLowerCase() === credential.toLowerCase() || 
            user.email.toLowerCase() === credential.toLowerCase()
        );

        if (foundUser) {
            showNotification(`Link de recuperação enviado para ${foundUser.email}!`, 'success');
        } else {
            showNotification('Se este usuário existir, um link foi enviado.', 'info');
        }
        
        hideButtonLoading('forgotForm');
        
        // Voltar ao login após 3 segundos
        setTimeout(() => {
            showAuthForm('login');
            document.getElementById('forgotCredential').value = '';
        }, 3000);
        
    }, 1500);
}

// ===========================================
// LOGIN SOCIAL (SIMULADO)
// ===========================================

/**
 * Simula login com Google
 */
function loginWithGoogle() {
    showNotification('Conectando com Google...', 'info');
    
    setTimeout(() => {
        const googleUser = {
            nick: 'GoogleUser' + Date.now().toString().slice(-4),
            email: 'usuario@gmail.com',
            provider: 'google',
            joinDate: new Date().toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' })
        };
        
        saveUserSession(googleUser);
        showNotification('Login com Google realizado!', 'success');
        
        setTimeout(() => {
            window.location.href = 'index.html';
        }, 1500);
    }, 1000);
}

/**
 * Simula login com Facebook
 */
function loginWithFacebook() {
    showNotification('Conectando com Facebook...', 'info');
    
    setTimeout(() => {
        const facebookUser = {
            nick: 'FacebookUser' + Date.now().toString().slice(-4),
            email: 'usuario@facebook.com',
            provider: 'facebook',
            joinDate: new Date().toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' })
        };
        
        saveUserSession(facebookUser);
        showNotification('Login com Facebook realizado!', 'success');
        
        setTimeout(() => {
            window.location.href = 'index.html';
        }, 1500);
    }, 1000);
}

/**
 * Simula cadastro com Google
 */
function registerWithGoogle() {
    loginWithGoogle();
}

/**
 * Simula cadastro com Facebook
 */
function registerWithFacebook() {
    loginWithFacebook();
}

// ===========================================
// VALIDAÇÕES E UTILITÁRIOS
// ===========================================

/**
 * Verifica se nick está em uso
 * @param {string} nick - Nick para verificar
 * @returns {boolean} True se já existe
 */
function isNickTaken(nick) {
    return usersDatabase.some(user => 
        user.nick.toLowerCase() === nick.toLowerCase()
    );
}

/**
 * Verifica se email está cadastrado
 * @param {string} email - Email para verificar
 * @returns {boolean} True se já existe
 */
function isEmailTaken(email) {
    return usersDatabase.some(user => 
        user.email.toLowerCase() === email.toLowerCase()
    );
}

/**
 * Calcula força da senha
 * @param {string} password - Senha para avaliar
 * @returns {string} 'weak', 'medium' ou 'strong'
 */
function getPasswordStrength(password) {
    if (!password) return 'weak';
    
    let strength = 0;
    
    // Comprimento
    if (password.length >= 6) strength++;
    if (password.length >= 8) strength++;
    
    // Caracteres especiais
    if (/[A-Z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[^A-Za-z0-9]/.test(password)) strength++;
    
    if (strength <= 2) return 'weak';
    if (strength <= 3) return 'medium';
    return 'strong';
}

/**
 * Salva sessão do usuário
 * @param {Object} user - Dados do usuário
 */
function saveUserSession(user) {
    // Em um sistema real, salvaria no localStorage ou cookie
    // Aqui apenas simula o processo
    console.log('Sessão salva:', user);
    
    // Simular salvamento de sessão
    const sessionData = {
        user: user,
        timestamp: new Date().toISOString(),
        sessionId: 'session_' + Date.now()
    };
    
    // Em produção, usaria localStorage aqui
    console.log('Dados da sessão:', sessionData);
}

/**
 * Adiciona classe de validação ao campo
 * @param {string} fieldId - ID do campo
 * @param {string} type - Tipo de validação ('error' ou 'success')
 */
function addValidationClass(fieldId, type) {
    const field = document.getElementById(fieldId);
    if (!field) return;
    
    const wrapper = field.closest('.input-wrapper');
    if (wrapper) {
        wrapper.classList.remove('error', 'success');
        wrapper.classList.add(type);
        
        // Remover classe após alguns segundos
        setTimeout(() => {
            wrapper.classList.remove(type);
        }, 3000);
    }
}

/**
 * Alterna visibilidade da senha
 * @param {string} fieldId - ID do campo de senha
 */
function togglePasswordVisibility(fieldId) {
    const field = document.getElementById(fieldId);
    const button = field.nextElementSibling;
    
    if (field.type === 'password') {
        field.type = 'text';
        button.textContent = '🙈';
    } else {
        field.type = 'password';
        button.textContent = '👁️';
    }
}

/**
 * Preenche campos de login com dados de demonstração
 * @param {string} nick - Nick do usuário
 * @param {string} password - Senha do usuário
 */
function fillLogin(nick, password) {
    document.getElementById('loginCredential').value = nick;
    document.getElementById('loginPassword').value = password;
    
    // Mostrar feedback visual
    showNotification(`Campos preenchidos com ${nick}`, 'info');
    
    // Focar no botão de login
    setTimeout(() => {
        const loginButton = document.querySelector('#loginForm button[type="submit"]');
        if (loginButton) {
            loginButton.focus();
        }
    }, 100);
}

// ===========================================
// INTERFACE E FEEDBACK
// ===========================================

/**
 * Mostra loading no botão do formulário
 * @param {string} formId - ID do formulário
 */
function showButtonLoading(formId) {
    const form = document.getElementById(formId);
    const button = form.querySelector('button[type="submit"]');
    const btnText = button.querySelector('.btn-text');
    const btnLoading = button.querySelector('.btn-loading');
    
    button.disabled = true;
    btnText.style.display = 'none';
    btnLoading.style.display = 'flex';
}

/**
 * Esconde loading do botão
 * @param {string} formId - ID do formulário
 */
function hideButtonLoading(formId) {
    const form = document.getElementById(formId);
    const button = form.querySelector('button[type="submit"]');
    const btnText = button.querySelector('.btn-text');
    const btnLoading = button.querySelector('.btn-loading');
    
    button.disabled = false;
    btnText.style.display = 'inline';
    btnLoading.style.display = 'none';
}

/**
 * Exibe notificação
 * @param {string} message - Mensagem da notificação
 * @param {string} type - Tipo ('success', 'error', 'warning', 'info')
 */
function showNotification(message, type = 'info') {
    const notification = document.getElementById('notification');
    const text = notification.querySelector('.notification-text');
    
    // Remover classes anteriores
    notification.classList.remove('success', 'error', 'warning', 'info');
    
    // Configurar notificação
    text.textContent = message;
    notification.classList.add(type);
    notification.classList.remove('hidden');
    
    // Auto-hide após 4 segundos
    setTimeout(() => {
        hideNotification();
    }, 4000);
}

/**
 * Esconde notificação
 */
function hideNotification() {
    const notification = document.getElementById('notification');
    notification.classList.add('hidden');
}

// ===========================================
// EVENT LISTENERS
// ===========================================

/**
 * Configura todos os event listeners
 */
function setupEventListeners() {
    // Validação em tempo real da força da senha
    const registerPassword = document.getElementById('registerPassword');
    if (registerPassword) {
        registerPassword.addEventListener('input', function() {
            updatePasswordStrength(this.value);
        });
    }

    // Validação da confirmação de senha
    const confirmPassword = document.getElementById('confirmPassword');
    if (confirmPassword) {
        confirmPassword.addEventListener('input', function() {
            validatePasswordMatch();
        });
    }

    // Validação de e-mail em tempo real
    const emailFields = document.querySelectorAll('input[type="email"]');
    emailFields.forEach(field => {
        field.addEventListener('blur', function() {
            if (this.value && !validateEmail(this.value)) {
                addValidationClass(this.id, 'error');
            } else if (this.value) {
                addValidationClass(this.id, 'success');
            }
        });
    });

    // Enter para submeter formulários
    document.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            const activeForm = document.querySelector('.auth-form.active');
            if (activeForm) {
                const submitButton = activeForm.querySelector('button[type="submit"]');
                if (submitButton && !submitButton.disabled) {
                    submitButton.click();
                }
            }
        }
    });

    // Limpar validações ao focar nos campos
    document.querySelectorAll('input').forEach(input => {
        input.addEventListener('focus', function() {
            const wrapper = this.closest('.input-wrapper');
            if (wrapper) {
                wrapper.classList.remove('error', 'success');
            }
        });
    });
}

/**
 * Atualiza indicador de força da senha
 * @param {string} password - Senha atual
 */
function updatePasswordStrength(password) {
    const strength = getPasswordStrength(password);
    const strengthFill = document.getElementById('passwordStrength');
    const strengthText = document.getElementById('passwordStrengthText');
    
    if (!strengthFill || !strengthText) return;
    
    // Remover classes anteriores
    strengthFill.classList.remove('weak', 'medium', 'strong');
    strengthText.classList.remove('weak', 'medium', 'strong');
    
    // Adicionar nova classe
    strengthFill.classList.add(strength);
    strengthText.classList.add(strength);
    
    // Atualizar texto
    const strengthTexts = {
        weak: 'Senha fraca',
        medium: 'Senha média',
        strong: 'Senha forte'
    };
    
    strengthText.textContent = password ? strengthTexts[strength] : 'Digite uma senha';
}

/**
 * Valida se as senhas coincidem
 */
function validatePasswordMatch() {
    const password = document.getElementById('registerPassword').value;
    const confirm = document.getElementById('confirmPassword').value;
    const matchIcon = document.getElementById('passwordMatch');
    
    if (!confirm) {
        matchIcon.textContent = '';
        return;
    }
    
    if (password === confirm) {
        matchIcon.textContent = '✅';
        addValidationClass('confirmPassword', 'success');
    } else {
        matchIcon.textContent = '❌';
        addValidationClass('confirmPassword', 'error');
    }
}

/**
 * Mostra termos de uso (simulado)
 */
function showTerms() {
    showNotification('Termos de Uso: Em construção...', 'info');
}

/**
 * Mostra política de privacidade (simulado)
 */
function showPrivacy() {
    showNotification('Política de Privacidade: Em construção...', 'info');
}

// ===========================================
// INICIALIZAÇÃO
// ===========================================

/**
 * Inicializa a página de login
 */
function init() {
    console.log('🔑 Iniciando tela de login...');
    
    // Configurar eventos
    setupEventListeners();
    
    // Verificar se já está logado
    checkExistingSession();
    
    // Efeitos visuais iniciais
    startVisualEffects();
    
    console.log('✅ Tela de login iniciada!');
}

/**
 * Verifica se já existe sessão ativa
 */
function checkExistingSession() {
    // Em um sistema real, verificaria localStorage/cookies
    // Para demonstração, não implementamos persistência
    console.log('🔍 Verificando sessão existente...');
}

/**
 * Inicia efeitos visuais
 */
function startVisualEffects() {
    // Adicionar animação aos livros flutuantes
    const floatingBooks = document.querySelectorAll('.floating-book');
    floatingBooks.forEach((book, index) => {
        book.style.animationDelay = `${index * 2}s`;
    });
    
    // Efeito de digitação no primeiro campo
    setTimeout(() => {
        const firstInput = document.querySelector('#loginForm input');
        if (firstInput) {
            firstInput.focus();
        }
    }, 500);
}

// Inicializar quando DOM estiver carregado
document.addEventListener('DOMContentLoaded', init);