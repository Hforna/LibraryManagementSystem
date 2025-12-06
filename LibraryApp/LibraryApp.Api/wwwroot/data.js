const appConfig = {
    name: "O Caminho do Saber",
    version: "2.0.0",
    description: "Biblioteca Digital Gratuita",
    maxFileSize: 50 * 1024 * 1024, // 50MB em bytes
    supportedFormats: ['.pdf', '.epub', '.txt', '.mobi'],
    defaultTheme: 'light',
    animationDuration: 300, // milissegundos
    searchDelay: 500, // milissegundos para debounce na busca
    statsUpdateInterval: 30000, // 30 segundos
    autoSaveInterval: 60000 // 1 minuto para auto-save
};

//mensagens e textos do site
const messages = {
    welcome: "Bem-vindo à O Caminho do Saber!",
    loginSuccess: "Login realizado com sucesso!",
    registerSuccess: "Cadastro realizado com sucesso!",
    logoutConfirm: "Tem certeza que deseja sair?",
    uploadSuccess: "Livro enviado com sucesso!",
    favoriteAdded: "Livro adicionado aos favoritos!",
    favoriteRemoved: "Livro removido dos favoritos!",
    profileUpdated: "Perfil atualizado com sucesso!",
    passwordChanged: "Senha alterada com sucesso!",
    goalUpdated: "Meta de leitura atualizada!",
    errors: {
        emptyFields: "Por favor, preencha todos os campos obrigatórios.",
        passwordMismatch: "As senhas não coincidem.",
        invalidEmail: "Por favor, digite um e-mail válido.",
        nickTaken: "Este nick já está em uso. Escolha outro.",
        emailTaken: "Este e-mail já está cadastrado.",
        loginRequired: "Por favor, faça login para acessar este recurso.",
        fileTooBig: "O arquivo é muito grande. Tamanho máximo: 50MB.",
        invalidFormat: "Formato de arquivo não suportado.",
        networkError: "Erro de conexão. Verifique sua internet.",
        serverError: "Erro no servidor. Tente novamente mais tarde.",
        unauthorized: "Sessão expirada. Faça login novamente.",
        notFound: "Recurso não encontrado."
    }
};

//ícone de categoria
const categoryIcons = {
    'literatura': '📚',
    'ficção': '📖',
    'não-ficção': '📝',
    'ciência': '🔬',
    'história': '📜',
    'filosofia': '🤔',
    'tecnologia': '💻',
    'autoajuda': '🌟',
    'biografia': '👤',
    'romance': '❤️',
    'suspense': '🔍',
    'fantasia': '🐉',
    'técnico': '🛠️',
    'educação': '🎓',
    'negócios': '💼',
    'arte': '🎨',
    'música': '🎵',
    'esportes': '⚽',
    'culinária': '🍳',
    'viagem': '✈️',
    'default': '📖'
};

//Retorna o ícone correspondente à categoria
function getCategoryIcon(category) {
    if (!category) return categoryIcons.default;
    const normalized = category.toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, ''); // Remove acentos
    return categoryIcons[normalized] || categoryIcons.default;
}

//gera um id único para  novos elementos
function generateUniqueId() {
    return Date.now() + Math.floor(Math.random() * 1000);
}

///formata números para exibir 
function formatNumber(num) {
    if (!num && num !== 0) return '0';
    return num.toLocaleString('pt-BR');
}

//valida formato do email
function validateEmail(email) {
    if (!email) return false;
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

///sanitiza string para evitar xss
function sanitizeString(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

//escapa html para exibição segura
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

//calcula tempo estimado de leitura
function calculateReadingTime(content) {
    if (!content) return 0;
    const wordsPerMinute = 200; // Média de leitura
    const wordCount = content.split(/\s+/).length;
    return Math.ceil(wordCount / wordsPerMinute);
}

//formata data para exibição
function formatDate(date) {
    if (!date) return 'Data não disponível';
    const d = new Date(date);
    return d.toLocaleDateString('pt-BR', {
        day: '2-digit',
        month: 'long',
        year: 'numeric'
    });
}

//formata o tamanho do arquivo
function formatFileSize(bytes) {
    if (!bytes || bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
}

//valida o tamanho do arquivo
function validateFileSize(file) {
    if (!file) return false;
    return file.size <= appConfig.maxFileSize;
}

//valida o formato do arquivo
function validateFileFormat(filename) {
    if (!filename) return false;
    const extension = '.' + filename.split('.').pop().toLowerCase();
    return appConfig.supportedFormats.includes(extension);
}

//trunca o texto com reticências
function truncateText(text, maxLength = 100) {
    if (!text) return '';
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
}

//debounce para funções
function debounce(func, wait = 300) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

//obtém mensagem de erro
function getFriendlyErrorMessage(error) {
    if (!error) return messages.errors.serverError;
    
    if (typeof error === 'string') return error;
    
    if (error.message) {
        const msg = error.message.toLowerCase();
        
        if (msg.includes('network') || msg.includes('fetch')) {
            return messages.errors.networkError;
        }
        if (msg.includes('unauthorized') || msg.includes('401')) {
            return messages.errors.unauthorized;
        }
        if (msg.includes('not found') || msg.includes('404')) {
            return messages.errors.notFound;
        }
        
        return error.message;
    }
    
    return messages.errors.serverError;
}

//copia texto para área de transferência
async function copyToClipboard(text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Erro ao copiar:', err);
        return false;
    }
}

//gera cor aleatória para categoria
function generateRandomColor() {
    const colors = [
        '#e74c3c', '#3498db', '#f39c12', '#9b59b6',
        '#1abc9c', '#f1c40f', '#34495e', '#e67e22',
        '#2ecc71', '#c0392b', '#16a085', '#d35400'
    ];
    return colors[Math.floor(Math.random() * colors.length)];
}

//verifica se é mobile
function isMobileDevice() {
    return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
}

//scroll suave para elemento
function smoothScrollTo(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
}

//obtém parâ^metro de url
function getUrlParameter(param) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
}

//define título da página
function setPageTitle(title) {
    document.title = title ? `${title} - ${appConfig.name}` : appConfig.name;
}

//sorteia array 
function shuffleArray(array) {
    const shuffled = [...array];
    for (let i = shuffled.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
    }
    return shuffled;
}

//agrupa array pro propriedade
function groupBy(array, key) {
    return array.reduce((result, item) => {
        const group = item[key];
        if (!result[group]) {
            result[group] = [];
        }
        result[group].push(item);
        return result;
    }, {});
}

//remove duplicatas de array
function removeDuplicates(array, key = null) {
    if (!key) {
        return [...new Set(array)];
    }
    const seen = new Set();
    return array.filter(item => {
        const value = item[key];
        if (seen.has(value)) {
            return false;
        }
        seen.add(value);
        return true;
    });
}

//valida força da senha
function validatePasswordStrength(password) {
    if (!password) {
        return { strength: 'weak', message: 'Digite uma senha' };
    }

    let strength = 0;
    
    if (password.length >= 6) strength++;
    if (password.length >= 8) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[^A-Za-z0-9]/.test(password)) strength++;

    if (strength <= 2) {
        return { strength: 'weak', message: 'Senha fraca', score: strength };
    }
    if (strength <= 3) {
        return { strength: 'medium', message: 'Senha média', score: strength };
    }
    return { strength: 'strong', message: 'Senha forte', score: strength };
}

//valida campos obrigatórios
function validateRequiredFields(fields) {
    const errors = [];
    
    for (const [key, value] of Object.entries(fields)) {
        if (!value || (typeof value === 'string' && value.trim() === '')) {
            errors.push(`O campo ${key} é obrigatório`);
        }
    }
    
    return {
        isValid: errors.length === 0,
        errors: errors
    };
}

//log de desenvolvimento
function devLog(message, type = 'info') {
    if (window.location.hostname === 'localhost' || window.location.hostname.includes('dev')) {
        const styles = {
            info: 'color: #3498db',
            success: 'color: #27ae60',
            warn: 'color: #f39c12',
            error: 'color: #e74c3c'
        };
        console.log(`%c[O Caminho do Saber] ${message}`, styles[type] || styles.info);
    }
}

// Disponibilizar configurações e funções globalmente
window.appConfig = appConfig;
window.messages = messages;
window.categoryIcons = categoryIcons;

// Funções utilitárias
window.getCategoryIcon = getCategoryIcon;
window.generateUniqueId = generateUniqueId;
window.formatNumber = formatNumber;
window.validateEmail = validateEmail;
window.sanitizeString = sanitizeString;
window.escapeHtml = escapeHtml;
window.calculateReadingTime = calculateReadingTime;
window.formatDate = formatDate;
window.formatFileSize = formatFileSize;
window.validateFileSize = validateFileSize;
window.validateFileFormat = validateFileFormat;
window.truncateText = truncateText;
window.debounce = debounce;
window.getFriendlyErrorMessage = getFriendlyErrorMessage;
window.copyToClipboard = copyToClipboard;
window.generateRandomColor = generateRandomColor;
window.isMobileDevice = isMobileDevice;
window.smoothScrollTo = smoothScrollTo;
window.getUrlParameter = getUrlParameter;
window.setPageTitle = setPageTitle;
window.shuffleArray = shuffleArray;
window.groupBy = groupBy;
window.removeDuplicates = removeDuplicates;
window.validatePasswordStrength = validatePasswordStrength;
window.validateRequiredFields = validateRequiredFields;
window.devLog = devLog;

devLog('Configurações e utilitários carregados', 'success');