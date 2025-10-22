/*
 * BIBLIOTECA VIRTUAL - JAVASCRIPT DA TELA DE LOGIN (API REAL)
 * Integrado com backend .NET em https://localhost:5001
 */

// ===========================================
// CONTROLE DE FORMULÁRIOS
// ===========================================

function showAuthForm(formType) {
    document.querySelectorAll('.auth-tab').forEach(tab => tab.classList.remove('active'));
    document.querySelectorAll('.auth-form').forEach(form => form.classList.remove('active'));
    
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

function showForgotPassword() {
    showAuthForm('forgot');
}

// ===========================================
// SISTEMA DE LOGIN (API REAL)
// ===========================================

async function login(event) {
    event.preventDefault();
    
    const credential = document.getElementById('loginCredential').value.trim();
    const password = document.getElementById('loginPassword').value;

    if (!credential || !password) {
        showNotification('Por favor, preencha todos os campos.', 'error');
        return;
    }

    // Mostrar loading
    showButtonLoading('loginForm');

    try {
        // Chamar API real
        const result = await AuthService.login(credential, password);

        if (result.success) {
            showNotification(`Bem-vindo de volta, ${result.user.userName}!`, 'success');
            
            // Redirecionar após 1 segundo
            setTimeout(() => {
                window.location.href = 'index.html';
            }, 1000);
        } else {
            hideButtonLoading('loginForm');
            showNotification(result.error || 'Credenciais inválidas. Tente novamente.', 'error');
            addValidationClass('loginCredential', 'error');
            addValidationClass('loginPassword', 'error');
        }
    } catch (error) {
        hideButtonLoading('loginForm');
        showNotification('Erro ao conectar com o servidor. Tente novamente.', 'error');
        console.error('Erro no login:', error);
    }
}

// ===========================================
// SISTEMA DE CADASTRO (API REAL)
// ===========================================

async function register(event) {
    event.preventDefault();
    
    const nick = document.getElementById('registerNick').value.trim();
    const email = document.getElementById('registerEmail').value.trim();
    const password = document.getElementById('registerPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    const acceptTerms = document.getElementById('acceptTerms').checked;

    // Validações client-side
    if (!nick || !email || !password || !confirmPassword) {
        showNotification('Por favor, preencha todos os campos obrigatórios.', 'error');
        return;
    }

    if (nick.length < 4) {
        showNotification('O apelido deve ter no mínimo 4 caracteres.', 'error');
        addValidationClass('registerNick', 'error');
        return;
    }

    if (!validateEmail(email)) {
        showNotification('Por favor, digite um e-mail válido.', 'error');
        addValidationClass('registerEmail', 'error');
        return;
    }

    if (password.length < 8) {
        showNotification('A senha deve ter no mínimo 8 caracteres.', 'error');
        addValidationClass('registerPassword', 'error');
        return;
    }

    if (password !== confirmPassword) {
        showNotification('As senhas não coincidem.', 'error');
        addValidationClass('confirmPassword', 'error');
        return;
    }

    if (!acceptTerms) {
        showNotification('Você deve aceitar os termos de uso.', 'error');
        return;
    }

    // Mostrar loading
    showButtonLoading('registerForm');

    try {
        // Chamar API real
        const result = await AuthService.register(nick, email, password);

        if (result.success) {
            hideButtonLoading('registerForm');
            
            // Mostrar mensagem de sucesso com instrução sobre email
            showNotification(result.message || 'Conta criada! Verifique seu email.', 'success');
            
            // Limpar formulário
            document.getElementById('registerNick').value = '';
            document.getElementById('registerEmail').value = '';
            document.getElementById('registerPassword').value = '';
            document.getElementById('confirmPassword').value = '';
            document.getElementById('acceptTerms').checked = false;
            
            // Trocar para formulário de login após 3 segundos
            setTimeout(() => {
                showAuthForm('login');
                showNotification('Confirme seu email antes de fazer login.', 'info');
            }, 3000);
            
        } else {
            hideButtonLoading('registerForm');
            showNotification(result.error || 'Erro ao criar conta.', 'error');
        }
    } catch (error) {
        hideButtonLoading('registerForm');
        showNotification('Erro ao conectar com o servidor. Tente novamente.', 'error');
        console.error('Erro no cadastro:', error);
    }
}

// ===========================================
// RECUPERAÇÃO DE SENHA
// ===========================================

async function resetPassword(event) {
    event.preventDefault();
    
    const credential = document.getElementById('forgotCredential').value.trim();
    
    if (!credential) {
        showNotification('Por favor, digite seu e-mail.', 'error');
        return;
    }

    showButtonLoading('forgotForm');

    // Simular envio (backend não tem endpoint ainda)
    setTimeout(() => {
        showNotification('Se este e-mail estiver cadastrado, você receberá instruções.', 'info');
        hideButtonLoading('forgotForm');
        
        setTimeout(() => {
            showAuthForm('login');
            document.getElementById('forgotCredential').value = '';
        }, 3000);
    }, 1500);
}

// ===========================================
// LOGIN SOCIAL (API REAL)
// ===========================================

function loginWithGoogle() {
    showNotification('Redirecionando para Google...', 'info');
    
    // Redirecionar para endpoint de Google OAuth
    AuthService.loginWithGoogle();
}

function loginWithFacebook() {
    showNotification('Login com Facebook não disponível no momento.', 'warning');
}

function registerWithGoogle() {
    loginWithGoogle();
}

function registerWithFacebook() {
    loginWithFacebook();
}

// ===========================================
// VALIDAÇÕES E UTILITÁRIOS
// ===========================================

function getPasswordStrength(password) {
    if (!password) return 'weak';
    
    let strength = 0;
    
    if (password.length >= 6) strength++;
    if (password.length >= 8) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[^A-Za-z0-9]/.test(password)) strength++;
    
    if (strength <= 2) return 'weak';
    if (strength <= 3) return 'medium';
    return 'strong';
}

function addValidationClass(fieldId, type) {
    const field = document.getElementById(fieldId);
    if (!field) return;
    
    const wrapper = field.closest('.input-wrapper');
    if (wrapper) {
        wrapper.classList.remove('error', 'success');
        wrapper.classList.add(type);
        
        setTimeout(() => {
            wrapper.classList.remove(type);
        }, 3000);
    }
}

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

function fillLogin(nick, password) {
    document.getElementById('loginCredential').value = nick;
    document.getElementById('loginPassword').value = password;
    showNotification(`Campos preenchidos com ${nick}`, 'info');
    
    setTimeout(() => {
        const loginButton = document.querySelector('#loginForm button[type="submit"]');
        if (loginButton) loginButton.focus();
    }, 100);
}

// ===========================================
// INTERFACE E FEEDBACK
// ===========================================

function showButtonLoading(formId) {
    const form = document.getElementById(formId);
    const button = form.querySelector('button[type="submit"]');
    const btnText = button.querySelector('.btn-text');
    const btnLoading = button.querySelector('.btn-loading');
    
    button.disabled = true;
    btnText.style.display = 'none';
    btnLoading.style.display = 'flex';
}

function hideButtonLoading(formId) {
    const form = document.getElementById(formId);
    const button = form.querySelector('button[type="submit"]');
    const btnText = button.querySelector('.btn-text');
    const btnLoading = button.querySelector('.btn-loading');
    
    button.disabled = false;
    btnText.style.display = 'inline';
    btnLoading.style.display = 'none';
}

function showNotification(message, type = 'info') {
    const notification = document.getElementById('notification');
    const text = notification.querySelector('.notification-text');
    
    notification.classList.remove('success', 'error', 'warning', 'info');
    text.textContent = message;
    notification.classList.add(type);
    notification.classList.remove('hidden');
    
    setTimeout(() => {
        hideNotification();
    }, 4000);
}

function hideNotification() {
    const notification = document.getElementById('notification');
    notification.classList.add('hidden');
}

// ===========================================
// EVENT LISTENERS
// ===========================================

function setupEventListeners() {
    const registerPassword = document.getElementById('registerPassword');
    if (registerPassword) {
        registerPassword.addEventListener('input', function() {
            updatePasswordStrength(this.value);
        });
    }

    const confirmPassword = document.getElementById('confirmPassword');
    if (confirmPassword) {
        confirmPassword.addEventListener('input', function() {
            validatePasswordMatch();
        });
    }

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

    document.querySelectorAll('input').forEach(input => {
        input.addEventListener('focus', function() {
            const wrapper = this.closest('.input-wrapper');
            if (wrapper) {
                wrapper.classList.remove('error', 'success');
            }
        });
    });
}

function updatePasswordStrength(password) {
    const strength = getPasswordStrength(password);
    const strengthFill = document.getElementById('passwordStrength');
    const strengthText = document.getElementById('passwordStrengthText');
    
    if (!strengthFill || !strengthText) return;
    
    strengthFill.classList.remove('weak', 'medium', 'strong');
    strengthText.classList.remove('weak', 'medium', 'strong');
    
    strengthFill.classList.add(strength);
    strengthText.classList.add(strength);
    
    const strengthTexts = {
        weak: 'Senha fraca',
        medium: 'Senha média',
        strong: 'Senha forte'
    };
    
    strengthText.textContent = password ? strengthTexts[strength] : 'Digite uma senha';
}

function validatePasswordMatch() {
    const password = document.getElementById('registerPassword').value;
    const confirm = document.getElementById('confirmPassword').value;
    const matchIcon = document.getElementById('passwordMatch');
    
    if (!confirm){
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

function showTerms() {
    showNotification('Termos de Uso: Em construção...', 'info');
}

function showPrivacy() {
    showNotification('Política de Privacidade: Em construção...', 'info');
}

// ===========================================
// INICIALIZAÇÃO
// ===========================================

function init() {
    console.log('🔑 Iniciando tela de login...');
    
    setupEventListeners();
    checkExistingSession();
    startVisualEffects();
    
    console.log('✅ Tela de login iniciada!');
}

function checkExistingSession() {
    // Se já está autenticado, redirecionar para index
    if (AuthService && AuthService.isAuthenticated()) {
        const user = AuthService.getCurrentUser();
        showNotification(`Você já está logado como ${user.userName}!`, 'info');
        setTimeout(() => {
            window.location.href = 'index.html';
        }, 1500);
    }
}

function startVisualEffects() {
    const floatingBooks = document.querySelectorAll('.floating-book');
    floatingBooks.forEach((book, index) => {
        book.style.animationDelay = `${index * 2}s`;
    });
    
    setTimeout(() => {
        const firstInput = document.querySelector('#loginForm input');
        if (firstInput) {
            firstInput.focus();
        }
    }, 500);
}

// Inicializar quando DOM estiver carregado
document.addEventListener('DOMContentLoaded', init);