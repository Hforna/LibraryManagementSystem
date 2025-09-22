/*
 * BIBLIOTECA VIRTUAL - DADOS E CONFIGURAÇÕES
 * Arquivo responsável por armazenar todos os dados da aplicação
 * Inclui: livros, categorias, usuários e configurações gerais
 */

// ===========================================
// VARIÁVEIS GLOBAIS DA APLICAÇÃO
// ===========================================

let books = []; // Array principal de livros
let favorites = new Set(); // Set de livros favoritos do usuário
let currentlyReading = new Set(); // Set de livros sendo lidos
let currentFontSize = 16; // Tamanho da fonte no leitor
let nightMode = false; // Modo noturno do leitor
let currentUser = null; // Dados do usuário logado

// Estatísticas do usuário logado
let userStats = {
    booksRead: 0,          // Livros lidos completamente
    readingTime: 0,        // Tempo total de leitura em horas
    contributions: 0,      // Livros contribuídos pelo usuário
    yearlyGoal: 12        // Meta anual de livros
};

// ===========================================
// BANCO DE DADOS SIMULADO DE USUÁRIOS
// ===========================================

let usersDatabase = [
    { 
        nick: 'admin', 
        email: 'admin@biblioteca.com', 
        password: '123456', 
        joinDate: 'Dezembro 2024' 
    },
    { 
        nick: 'leitor01', 
        email: 'leitor@email.com', 
        password: 'senha123', 
        joinDate: 'Janeiro 2025' 
    },
    { 
        nick: 'bookworm', 
        email: 'bookworm@gmail.com', 
        password: 'livros2025', 
        joinDate: 'Janeiro 2025' 
    }
];

// ===========================================
// DADOS DOS LIVROS DE EXEMPLO
// ===========================================

const sampleBooks = [
    {
        id: 1,
        title: "Dom Casmurro",
        author: "Machado de Assis",
        category: "Literatura",
        icon: "📖",
        description: "Clássico da literatura brasileira que narra a história de Bentinho e Capitu",
        content: `
            <h3>Capítulo I - Do título</h3>
            <p>Uma noite destas, vindo da cidade para o Engenho Novo, encontrei no trem da Central um rapaz aqui do bairro, que eu conheço de vista e de chapéu. Cumprimentou-me, sentou-se ao pé de mim, falou da lua e dos ministros, e por fim deu-me parte de que ia casar.</p>
            
            <p>— Naturalmente com uma mocinha bonita, disse eu.</p>
            
            <p>— Bonita e moça, respondeu ele; mas não é isso só. É também muito sabida. Lê Victor Hugo no original.</p>
            
            <p>Este é o conteúdo simulado do famoso romance de Machado de Assis, considerado uma das maiores obras da literatura brasileira. A história é narrada por Bento Santiago, que relembra sua juventude e seu amor por Capitu...</p>
        `
    },
    {
        id: 2,
        title: "O Cortiço",
        author: "Aluísio Azevedo",
        category: "Literatura",
        icon: "🏘️",
        description: "Romance naturalista que retrata a vida em um cortiço no Rio de Janeiro",
        content: `
            <h3>Capítulo I</h3>
            <p>João Romão foi, dos treze aos vinte e cinco anos, empregado de um vendeiro que enriqueceu entre as quatro paredes de uma suja e obscura taverna nos refolhos do bairro do Botafogo; e tanto economizou do pouco que ganhava e tanto se estreitou, que, ao cabo de um tempo, achou-se com alguns mil cruzeiros...</p>
            
            <p>Este romance naturalista brasileiro apresenta um retrato fiel da sociedade do século XIX, mostrando as condições de vida das classes populares através da história de um cortiço...</p>
        `
    },
    {
        id: 3,
        title: "Física Quântica para Iniciantes",
        author: "Diversos Autores",
        category: "Ciência",
        icon: "⚛️",
        description: "Introdução aos conceitos fundamentais da mecânica quântica",
        content: `
            <h3>Capítulo 1 - Introdução à Física Quântica</h3>
            <p>A física quântica é um dos ramos mais fascinantes e revolucionários da física moderna. Desenvolvida no início do século XX, ela descreve o comportamento da matéria e da energia em escalas atômicas e subatômicas.</p>
            
            <h4>Princípios Fundamentais:</h4>
            <ul>
                <li><strong>Quantização da Energia:</strong> A energia não é contínua, mas vem em "pacotes" discretos chamados quanta.</li>
                <li><strong>Dualidade Onda-Partícula:</strong> Partículas podem exibir propriedades tanto de onda quanto de partícula.</li>
                <li><strong>Princípio da Incerteza:</strong> Não é possível determinar simultaneamente a posição e o momento de uma partícula com precisão absoluta.</li>
            </ul>
            
            <p>Este livro irá guiá-lo através dos conceitos fundamentais de forma acessível e didática...</p>
        `
    },
    {
        id: 4,
        title: "História do Brasil",
        author: "Boris Fausto",
        category: "História",
        icon: "🇧🇷",
        description: "Panorama completo da história brasileira desde o descobrimento",
        content: `
            <h3>Capítulo 1 - O Descobrimento do Brasil</h3>
            <p>Em 22 de abril de 1500, a esquadra comandada por Pedro Álvares Cabral avistou terras que mais tarde seriam conhecidas como Brasil. Este momento marca o início de uma nova era na história do continente americano.</p>
            
            <h4>Os Primeiros Contatos:</h4>
            <p>Os portugueses encontraram uma terra habitada por diversos povos indígenas, cada um com suas próprias culturas, línguas e tradições. O primeiro contato foi marcado tanto pela curiosidade mútua quanto por mal-entendidos culturais.</p>
            
            <p>A colonização que se seguiu transformaria profundamente tanto a terra quanto seus habitantes originais...</p>
        `
    },
    {
        id: 5,
        title: "Meditações",
        author: "Marco Aurélio",
        category: "Filosofia",
        icon: "🧘",
        description: "Reflexões filosóficas do imperador romano sobre estoicismo e vida",
        content: `
            <h3>Livro Primeiro</h3>
            <p><em>1.</em> De meu avô Vero aprendi a bondade e a serenidade de temperamento.</p>
            
            <p><em>2.</em> Da reputação e memória de meu pai, aprendi a modéstia e a força de caráter.</p>
            
            <p><em>3.</em> De minha mãe, aprendi a piedade, a generosidade e a abstenção não apenas de fazer mal, mas até mesmo de ter pensamentos maus...</p>
            
            <h4>Sobre a Vida e a Morte:</h4>
            <p>"Lembra-te constantemente de quantos médicos morreram depois de muitas vezes franzir o cenho sobre os doentes; quantos astrólogos depois de predizer com grande pompa a morte de outros..."</p>
            
            <p>Estas reflexões pessoais do imperador filósofo oferecem insights atemporais sobre como viver uma vida virtuosa...</p>
        `
    },
    {
        id: 6,
        title: "JavaScript Moderno",
        author: "Comunidade Dev",
        category: "Tecnologia",
        icon: "💻",
        description: "Guia completo de JavaScript ES6+ e programação moderna",
        content: `
            <h3>Capítulo 1 - Introdução ao JavaScript Moderno</h3>
            <p>JavaScript é uma das linguagens de programação mais populares e versáteis do mundo. Com o advento do ES6 (ECMAScript 2015) e versões posteriores, a linguagem ganhou recursos poderosos que a tornaram ainda mais expressiva e eficiente.</p>
            
            <h4>Novos Recursos do ES6+:</h4>
            <pre><code>
// Arrow Functions
const soma = (a, b) => a + b;

// Destructuring
const { nome, idade } = pessoa;

// Template Literals
const mensagem = \`Olá, \${nome}! Você tem \${idade} anos.\`;

// Promises e Async/Await
const dados = await fetch('/api/dados');
            </code></pre>
            
            <p>Este guia aborda desde conceitos básicos até técnicas avançadas de programação JavaScript...</p>
        `
    }
];

// ===========================================
// CONFIGURAÇÕES DAS CATEGORIAS
// ===========================================

const categories = [
    { 
        name: "Literatura", 
        icon: "📚", 
        count: 324,
        color: "#e74c3c"
    },
    { 
        name: "Ciência", 
        icon: "🔬", 
        count: 156,
        color: "#3498db"
    },
    { 
        name: "História", 
        icon: "📜", 
        count: 289,
        color: "#f39c12"
    },
    { 
        name: "Filosofia", 
        icon: "🤔", 
        count: 145,
        color: "#9b59b6"
    },
    { 
        name: "Tecnologia", 
        icon: "💻", 
        count: 98,
        color: "#1abc9c"
    },
    { 
        name: "Autoajuda", 
        icon: "🌟", 
        count: 67,
        color: "#f1c40f"
    },
    { 
        name: "Biografia", 
        icon: "👤", 
        count: 89,
        color: "#34495e"
    },
    { 
        name: "Ficção", 
        icon: "🌌", 
        count: 234,
        color: "#e67e22"
    }
];

// ===========================================
// MAPEAMENTO DE ÍCONES POR CATEGORIA
// ===========================================

const categoryIcons = {
    'literatura': '📚',
    'ciencia': '🔬',
    'historia': '📜',
    'filosofia': '🤔',
    'tecnologia': '💻',
    'autoajuda': '🌟',
    'biografia': '👤',
    'ficcao': '🌌',
    'default': '📖'
};

// ===========================================
// CONFIGURAÇÕES GERAIS DA APLICAÇÃO
// ===========================================

const appConfig = {
    name: "BiblioLivre",
    version: "1.0.0",
    description: "Biblioteca Digital Gratuita",
    maxFileSize: 50 * 1024 * 1024, // 50MB em bytes
    supportedFormats: ['.pdf', '.epub', '.txt', '.mobi'],
    defaultTheme: 'light',
    animationDuration: 300, // milissegundos
    searchDelay: 500, // milissegundos para debounce na busca
    statsUpdateInterval: 30000, // 30 segundos
    autoSaveInterval: 60000 // 1 minuto para auto-save
};

// ===========================================
// MENSAGENS E TEXTOS DA INTERFACE
// ===========================================

const messages = {
    welcome: "Bem-vindo à BiblioLivre!",
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
        invalidFormat: "Formato de arquivo não suportado."
    }
};

// ===========================================
// FUNÇÕES UTILITÁRIAS PARA DADOS
// ===========================================

/**
 * Retorna o ícone correspondente à categoria
 * @param {string} category - Nome da categoria
 * @returns {string} Ícone da categoria
 */
function getCategoryIcon(category) {
    return categoryIcons[category.toLowerCase()] || categoryIcons.default;
}

/**
 * Gera um ID único para novos elementos
 * @returns {number} ID único baseado em timestamp
 */
function generateUniqueId() {
    return Date.now() + Math.random();
}

/**
 * Formata números para exibição (ex: 1234 -> 1.234)
 * @param {number} num - Número para formatar
 * @returns {string} Número formatado
 */
function formatNumber(num) {
    return num.toLocaleString('pt-BR');
}

/**
 * Valida formato de e-mail
 * @param {string} email - E-mail para validar
 * @returns {boolean} True se válido
 */
function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

/**
 * Sanitiza string para evitar XSS
 * @param {string} str - String para sanitizar
 * @returns {string} String sanitizada
 */
function sanitizeString(str) {
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

/**
 * Calcula tempo estimado de leitura
 * @param {string} content - Conteúdo do livro
 * @returns {number} Tempo em minutos
 */
function calculateReadingTime(content) {
    const wordsPerMinute = 200; // Média de leitura
    const wordCount = content.split(/\s+/).length;
    return Math.ceil(wordCount / wordsPerMinute);
}

/**
 * Obtém livros por categoria
 * @param {string} category - Nome da categoria
 * @returns {Array} Array de livros da categoria
 */
function getBooksByCategory(category) {
    return books.filter(book => 
        book.category.toLowerCase() === category.toLowerCase()
    );
}

/**
 * Busca livros por termo
 * @param {string} query - Termo de busca
 * @returns {Array} Array de livros encontrados
 */
function searchBooksData(query) {
    const searchTerm = query.toLowerCase();
    return books.filter(book =>
        book.title.toLowerCase().includes(searchTerm) ||
        book.author.toLowerCase().includes(searchTerm) ||
        book.category.toLowerCase().includes(searchTerm) ||
        book.description.toLowerCase().includes(searchTerm)
    );
}