// INICIALIZAÇÃO DA APLICAÇÃO

//Função principal de inicialização que executa quando a aplicação carrega

function init() {
    console.log('🚀 Iniciando BiblioLivre...');
    
    // Carregar dados iniciais
    books = [...sampleBooks];
    
    // Renderizar interface inicial
    renderBooks(books);
    renderCategories();
    updateStats();
    
    // Verificar estado de autenticação
    checkAuthState();
    
    // Adicionar estilos dinâmicos
    addDynamicStyles();
    
    // Configurar eventos
    setupEventListeners();
    
    // Iniciar processos automáticos
    startAutomaticProcesses();
    
    // Mostrar informações de desenvolvimento no console
    showDevInfo();
    
    console.log('✅ BiblioLivre iniciado com sucesso!');
}

//Configura todos os event listeners da aplicação
function setupEventListeners() {
    // Event listener para busca com Enter
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                searchBooks();
            }
        });

        // Busca em tempo real (com debounce)
        let searchTimeout;
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                if (this.value.length >= 2) {
                    searchBooks();
                } else if (this.value.length === 0) {
                    renderBooks(books);
                }
            }, appConfig.searchDelay);
        });
    }

    // Event listeners para modais
    setupModalEventListeners();
    
    // Event listeners para teclado
    setupKeyboardEventListeners();
    
    // Event listeners para formulários
    setupFormEventListeners();
}

/**
 * Configura eventos dos modais
 */
function setupModalEventListeners() {
    // Fechar modal clicando fora dele
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('modal')) {
            if (e.target.id === 'authModal') {
                closeAuthModal();
            } else if (e.target.id === 'editProfileModal') {
                closeEditProfile();
            }
        }
    });

    // Prevenir fechamento ao clicar no conteúdo do modal
    document.querySelectorAll('.modal-content').forEach(modal => {
        modal.addEventListener('click', function(e) {
            e.stopPropagation();
        });
    });
}


 //Configura atalhos de teclado

function setupKeyboardEventListeners() {
    document.addEventListener('keydown', function(e) {
        // ESC - Fechar modais e leitor
        if (e.key === 'Escape') {
            if (document.getElementById('readerContainer').style.display === 'block') {
                closeReader();
            } else if (document.getElementById('authModal').classList.contains('show')) {
                closeAuthModal();
            } else if (document.getElementById('editProfileModal').classList.contains('show')) {
                closeEditProfile();
            }
        }

        // Ctrl+F - Focar na busca
        if (e.ctrlKey && e.key === 'f') {
            e.preventDefault();
            document.getElementById('searchInput').focus();
        }

        // Ctrl+L - Abrir modal de login
        if (e.ctrlKey && e.key === 'l' && !currentUser) {
            e.preventDefault();
            showLoginModal();
        }
    });
}


 //Configura eventos dos formulários

function setupFormEventListeners() {
    // Validação em tempo real para campos de e-mail
    document.querySelectorAll('input[type="email"]').forEach(input => {
        input.addEventListener('blur', function() {
            if (this.value && !validateEmail(this.value)) {
                this.style.borderColor = '#dc3545';
                showNotification('E-mail inválido', 'error');
            } else {
                this.style.borderColor = '#28a745';
            }
        });
    });

    // Validação de confirmação de senha
    const confirmPasswordFields = ['confirmPassword', 'confirmNewPassword'];
    confirmPasswordFields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) {
            field.addEventListener('input', function() {
                const passwordField = fieldId === 'confirmPassword' ? 
                    document.getElementById('registerPassword') :
                    document.getElementById('newPassword');
                
                if (passwordField && this.value) {
                    if (this.value === passwordField.value) {
                        this.style.borderColor = '#28a745';
                    } else {
                        this.style.borderColor = '#dc3545';
                    }
                }
            });
        }
    });
}

// PROCESSOS AUTOMÁTICOS

//Inicia processos que executam automaticamente

function startAutomaticProcesses() {
    // Atualizar estatísticas periodicamente
    setInterval(() => {
        updateStats();
    }, appConfig.statsUpdateInterval);

    // Adicionar livros aleatórios para demonstração
    setInterval(() => {
        addRandomBooksDemo();
    }, 120000); // A cada 2 minutos

    // Auto-save do progresso do usuário
    if (currentUser) {
        setInterval(() => {
            autoSaveUserProgress();
        }, appConfig.autoSaveInterval);
    }
}


 //Adiciona livros aleatórios para demonstração

function addRandomBooksDemo() {
    const randomTitles = [
        { 
            title: "1984", 
            author: "George Orwell", 
            category: "Literatura", 
            icon: "👁️",
            description: "Distopia clássica sobre vigilância e controle totalitário"
        },
        { 
            title: "Sapiens", 
            author: "Yuval Harari", 
            category: "História", 
            icon: "🧬",
            description: "Uma breve história da humanidade"
        },
        { 
            title: "Python para Todos", 
            author: "Comunidade Tech", 
            category: "Tecnologia", 
            icon: "🐍",
            description: "Aprenda programação Python do zero"
        },
        { 
            title: "O Poder do Agora", 
            author: "Eckhart Tolle", 
            category: "Autoajuda", 
            icon: "⏰",
            description: "Um guia para a iluminação espiritual"
        },
        { 
            title: "Cosmos", 
            author: "Carl Sagan", 
            category: "Ciência", 
            icon: "🌌",
            description: "Uma viagem pessoal através do universo"
        },
        {
            title: "A Arte da Guerra",
            author: "Sun Tzu",
            category: "Filosofia",
            icon: "⚔️",
            description: "Estratégias milenares aplicadas ao mundo moderno"
        }
    ];

    const randomBook = randomTitles[Math.floor(Math.random() * randomTitles.length)];
    
    // Verificar se o livro já existe
    if (!books.find(b => b.title === randomBook.title)) {
        const newBook = {
            ...randomBook,
            id: generateUniqueId(),
            content: generateSampleContent(randomBook.title, randomBook.author, randomBook.description),
            uploadedBy: 'Sistema',
            uploadDate: new Date().toLocaleDateString('pt-BR')
        };
        
        books.push(newBook);
        
        // Atualizar interface se estivermos na aba home
        const homeTab = document.getElementById('home');
        if (homeTab && homeTab.classList.contains('active')) {
            renderBooks(books);
        }
        
        updateStats();
        
        // Mostrar notificação discreta
        setTimeout(() => {
            showNotification(`📚 Novo livro adicionado: "${randomBook.title}"`, 'info');
        }, 1000);
    }
}


//Auto-save do progresso do usuário (simulado)

function autoSaveUserProgress() {
    if (!currentUser) return;
    
    // Simular salvamento automático
    console.log(`💾 Auto-save: Progresso de ${currentUser.name} salvo automaticamente`);
    
    // Em um sistema real, enviaria dados para o servidor
    const progressData = {
        userId: currentUser.id,
        favorites: Array.from(favorites),
        currentlyReading: Array.from(currentlyReading),
        userStats: userStats,
        timestamp: new Date().toISOString()
    };
    
    // Simular salvamento local (localStorage seria usado aqui)
    console.log('Dados salvos:', progressData);
}

// INFORMAÇÕES DE DESENVOLVIMENTO


 //Exibe informações úteis no console para desenvolvimento

function showDevInfo() {
    console.group('🔐 Usuários de Teste Disponíveis:');
    usersDatabase.forEach(user => {
        console.log(`👤 Nick: ${user.nick} | E-mail: ${user.email} | Senha: ${user.password}`);
    });
    console.groupEnd();

    console.group('📊 Estatísticas Iniciais:');
    console.log(`📚 Livros carregados: ${books.length}`);
    console.log(`📂 Categorias disponíveis: ${categories.length}`);
    console.log(`👥 Usuários cadastrados: ${usersDatabase.length}`);
    console.groupEnd();

    console.group('🛠️ Configurações:');
    console.log('Versão:', appConfig.version);
    console.log('Formatos suportados:', appConfig.supportedFormats.join(', '));
    console.log('Tamanho máximo de arquivo:', (appConfig.maxFileSize / 1024 / 1024).toFixed(1) + 'MB');
    console.groupEnd();

    console.group('⌨️ Atalhos de Teclado:');
    console.log('ESC - Fechar modais/leitor');
    console.log('Ctrl+F - Focar na busca');
    console.log('Ctrl+L - Abrir login (se não logado)');
    console.groupEnd();

    // Informações sobre recursos em desenvolvimento
    console.group('🚧 Próximas Funcionalidades:');
    console.log('• Sistema de comentários nos livros');
    console.log('• Grupos de leitura');
    console.log('• Rankings e conquistas');
    console.log('• Modo offline');
    console.log('• App mobile');
    console.groupEnd();
}

// UTILITÁRIOS PARA DEMONSTRAÇÃO

//Simula atividade de usuários para demonstração

function simulateUserActivity() {
    // Incrementar leituras aleatoriamente
    const currentReads = parseInt(document.getElementById('totalReads').textContent.replace(/[.,]/g, ''));
    const increment = Math.floor(Math.random() * 5) + 1;
    animateNumber('totalReads', currentReads + increment);

    // Incrementar usuários ativos ocasionalmente
    if (Math.random() > 0.7) {
        const currentUsers = parseInt(document.getElementById('activeUsers').textContent.replace(/[.,]/g, ''));
        animateNumber('activeUsers', currentUsers + Math.floor(Math.random() * 3) + 1);
    }
}


//Executa demonstrações interativas

function runInteractiveDemos() {
    // Demonstração de busca automática (apenas para apresentação)
    if (window.location.search.includes('demo=true')) {
        setTimeout(() => {
            document.getElementById('searchInput').value = 'Machado';
            searchBooks();
            showNotification('🎭 Demonstração automática ativada!', 'info');
        }, 3000);
    }
}


//Limpa dados da demonstração

function resetDemoData() {
    if (confirm('⚠️ Isso irá limpar todos os dados da demonstração. Continuar?')) {
        books.splice(sampleBooks.length); // Manter apenas livros originais
        favorites.clear();
        currentlyReading.clear();
        currentUser = null;
        userStats = {
            booksRead: 0,
            readingTime: 0,
            contributions: 0,
            yearlyGoal: 12
        };
        
        updateAuthUI();
        renderBooks(books);
        updateStats();
        showNotification('🗑️ Dados da demonstração limpos!', 'success');
    }
}

// TRATAMENTO DE ERROS

//Manipulador global de erros

window.addEventListener('error', function(e) {
    console.error('❌ Erro capturado:', e.error);
    showNotification('Ops! Algo deu errado. Tente recarregar a página.', 'error');
});


//Manipulador para promessas rejeitadas

window.addEventListener('unhandledrejection', function(e) {
    console.error('❌ Promise rejeitada:', e.reason);
    showNotification('Erro de conectividade. Verifique sua conexão.', 'warning');
});

// INICIALIZAÇÃO AUTOMÁTICA

// Aguardar carregamento completo do DOM
document.addEventListener('DOMContentLoaded', function() {
    init();
    
    // Executar demonstrações se solicitado
    setTimeout(runInteractiveDemos, 1000);
    
    // Simular atividade periodicamente
    setInterval(simulateUserActivity, 45000);
});

// Garantir inicialização mesmo se DOMContentLoaded já passou
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
} else {
    init();
}

// EXPOSIÇÃO GLOBAL PARA DEBUG

// Expor funções úteis para debug no console
if (window.location.hostname === 'localhost' || window.location.hostname.includes('dev')) {
    window.bibliotecaDebug = {
        showAllUsers: () => console.table(usersDatabase),
        showAllBooks: () => console.table(books),
        resetDemo: resetDemoData,
        simulateUser: simulateUserActivity,
        addRandomBook: addRandomBooksDemo,
        showConfig: () => console.log(appConfig),
        showStats: () => console.log({
            books: books.length,
            users: usersDatabase.length,
            favorites: favorites.size,
            reading: currentlyReading.size
        })
    };
    
    console.log('🔍 Debug tools disponíveis em window.bibliotecaDebug');
}