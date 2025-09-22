/*
 * BIBLIOTECA VIRTUAL - INTERFACE E INTERAÇÕES
 * Arquivo responsável por toda a interface do usuário e interações
 * Inclui: renderização, navegação, busca, leitor e perfil
 */

// ===========================================
// SISTEMA DE NAVEGAÇÃO POR ABAS
// ===========================================

/**
 * Controla a navegação entre as diferentes seções
 * @param {string} tabName - Nome da aba a ser exibida
 */
function showTab(tabName) {
    // Esconder todos os conteúdos
    document.querySelectorAll('.tab-content').forEach(content => {
        content.classList.remove('active');
    });
    
    // Desativar todas as abas
    document.querySelectorAll('.tab').forEach(tab => {
        tab.classList.remove('active');
    });
    
    // Mostrar conteúdo selecionado
    document.getElementById(tabName).classList.add('active');
    event.target.classList.add('active');

    // Atualizar conteúdo específico da aba
    switch(tabName) {
        case 'home':
            renderBooks(books);
            break;
        case 'categories':
            renderCategories();
            break;
        case 'favorites':
            renderFavorites();
            break;
        case 'reading':
            renderCurrentlyReading();
            break;
        case 'profile':
            if (currentUser) {
                updateProfileData();
                renderProfileActivity();
            }
            break;
    }
}

// ===========================================
// RENDERIZAÇÃO DE LIVROS
// ===========================================

/**
 * Renderiza grid de livros na interface
 * @param {Array} booksToRender - Array de livros para renderizar
 */
function renderBooks(booksToRender) {
    const grid = document.getElementById('booksGrid');
    
    if (booksToRender.length === 0) {
        grid.innerHTML = '<p style="text-align: center; color: #666; grid-column: 1/-1;">Nenhum livro encontrado.</p>';
        return;
    }

    grid.innerHTML = booksToRender.map(book => createBookCard(book)).join('');
}

/**
 * Cria o HTML de um card de livro
 * @param {Object} book - Dados do livro
 * @returns {string} HTML do card
 */
function createBookCard(book) {
    const isFavorite = favorites.has(book.id);
    const favoriteIcon = isFavorite ? '❤️' : '🤍';
    const favoriteColor = isFavorite ? '#dc3545' : '#6c757d';
    
    return `
        <div class="book-card" data-book-id="${book.id}">
            <div class="book-cover">${book.icon}</div>
            <div class="book-title">${sanitizeString(book.title)}</div>
            <div class="book-author">por ${sanitizeString(book.author)}</div>
            <div class="book-category">${sanitizeString(book.category)}</div>
            <div class="book-actions">
                <button class="btn btn-read" onclick="readBook(${book.id})" title="Ler livro">Ler</button>
                <button class="btn btn-download" onclick="downloadBook(${book.id})" title="Baixar livro">Download</button>
                <button class="btn" onclick="toggleFavorite(${book.id})" 
                        style="background: ${favoriteColor}; color: white;" 
                        title="${isFavorite ? 'Remover dos favoritos' : 'Adicionar aos favoritos'}">
                    ${favoriteIcon}
                </button>
            </div>
        </div>
    `;
}

/**
 * Renderiza livros favoritos
 */
function renderFavorites() {
    const favoriteBooks = books.filter(book => favorites.has(book.id));
    const grid = document.getElementById('favoritesGrid');
    
    if (favoriteBooks.length === 0) {
        grid.innerHTML = '<p style="text-align: center; color: #666; grid-column: 1/-1;">Nenhum livro favoritado ainda. Explore nossa biblioteca e adicione seus favoritos!</p>';
    } else {
        grid.innerHTML = favoriteBooks.map(book => createBookCard(book)).join('');
    }
}

/**
 * Renderiza livros sendo lidos atualmente
 */
function renderCurrentlyReading() {
    const readingBooks = books.filter(book => currentlyReading.has(book.id));
    const grid = document.getElementById('readingGrid');
    
    if (readingBooks.length === 0) {
        grid.innerHTML = '<p style="text-align: center; color: #666; grid-column: 1/-1;">Nenhuma leitura em andamento. Comece a ler um livro para vê-lo aqui!</p>';
    } else {
        grid.innerHTML = readingBooks.map(book => createReadingBookCard(book)).join('');
    }
}

/**
 * Cria card especial para livros em leitura
 * @param {Object} book - Dados do livro
 * @returns {string} HTML do card
 */
function createReadingBookCard(book) {
    return `
        <div class="book-card" data-book-id="${book.id}">
            <div class="book-cover">${book.icon}</div>
            <div class="book-title">${sanitizeString(book.title)}</div>
            <div class="book-author">por ${sanitizeString(book.author)}</div>
            <div class="book-category">${sanitizeString(book.category)}</div>
            <div style="background: #28a745; color: white; padding: 5px; border-radius: 5px; margin: 10px 0; font-size: 0.8rem;">
                Em andamento
            </div>
            <div class="book-actions">
                <button class="btn btn-read" onclick="readBook(${book.id})">Continuar</button>
                <button class="btn btn-download" onclick="downloadBook(${book.id})">Download</button>
            </div>
        </div>
    `;
}

// ===========================================
// RENDERIZAÇÃO DE CATEGORIAS
// ===========================================

/**
 * Renderiza grid de categorias
 */
function renderCategories() {
    const grid = document.getElementById('categoriesGrid');
    grid.innerHTML = categories.map(category => createCategoryCard(category)).join('');
}

/**
 * Cria card de categoria
 * @param {Object} category - Dados da categoria
 * @returns {string} HTML do card
 */
function createCategoryCard(category) {
    return `
        <div class="category-card" onclick="filterByCategory('${category.name}')" 
             style="background: linear-gradient(135deg, ${category.color}88, ${category.color}cc)">
            <div class="category-icon">${category.icon}</div>
            <div class="category-name">${category.name}</div>
            <div style="font-size: 0.9rem; opacity: 0.8;">${formatNumber(category.count)} livros</div>
        </div>
    `;
}

// ===========================================
// SISTEMA DE BUSCA
// ===========================================

/**
 * Executa busca de livros
 */
function searchBooks() {
    const query = document.getElementById('searchInput').value.trim();
    
    if (query.length === 0) {
        renderBooks(books);
        return;
    }

    const filtered = searchBooksData(query);
    renderBooks(filtered);
    
    // Mostrar aba home se estivermos em outra aba
    const homeTab = document.querySelector('.tab[onclick="showTab(\'home\')"]');
    if (homeTab) {
        homeTab.click();
    }

    // Feedback visual da busca
    const searchBox = document.getElementById('searchInput');
    searchBox.style.borderColor = filtered.length > 0 ? '#28a745' : '#dc3545';
    setTimeout(() => {
        searchBox.style.borderColor = '#e9ecef';
    }, 1000);
}

/**
 * Filtra livros por categoria
 * @param {string} categoryName - Nome da categoria
 */
function filterByCategory(categoryName) {
    const filtered = getBooksByCategory(categoryName);
    renderBooks(filtered);
    
    // Atualizar campo de busca
    document.getElementById('searchInput').value = `Categoria: ${categoryName}`;
    
    // Mostrar aba home
    const homeTab = document.querySelector('.tab[onclick="showTab(\'home\')"]');
    if (homeTab) {
        homeTab.click();
    }
}

// ===========================================
// SISTEMA DE FAVORITOS
// ===========================================

/**
 * Alterna status de favorito de um livro
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
    renderBooks(books);
    updateProfileData();
}

// ===========================================
// LEITOR INTEGRADO
// ===========================================

/**
 * Abre o leitor para um livro específico
 * @param {number} bookId - ID do livro
 */
function readBook(bookId) {
    const book = books.find(b => b.id === bookId);
    if (!book) return;

    // Adicionar à lista de leitura atual
    currentlyReading.add(bookId);
    
    // Simular tempo de leitura e atualizar estatísticas se logado
    if (currentUser) {
        userStats.readingTime += Math.floor(Math.random() * 3) + 1; // 1-3 horas
        userStats.booksRead++;
        updateProfileData();
    }
    
    // Configurar leitor
    document.getElementById('readerTitle').textContent = book.title;
    document.getElementById('readerContent').innerHTML = `
        <div style="margin-bottom: 20px;">
            <h2>${sanitizeString(book.title)}</h2>
            <p><strong>Autor:</strong> ${sanitizeString(book.author)}</p>
            <p><strong>Categoria:</strong> ${sanitizeString(book.category)}</p>
            <p><strong>Descrição:</strong> ${sanitizeString(book.description)}</p>
            <p><strong>Tempo estimado:</strong> ${calculateReadingTime(book.content)} minutos</p>
        </div>
        <hr style="margin: 20px 0;">
        <div style="line-height: 1.8; font-size: ${currentFontSize}px; font-family: Georgia, serif;">
            ${book.content}
        </div>
    `;
    
    // Mostrar leitor
    document.getElementById('readerContainer').style.display = 'block';
    document.querySelector('.content-section').style.display = 'none';
    
    showNotification(`Iniciando leitura: "${book.title}"`, 'success');
}

/**
 * Fecha o leitor
 */
function closeReader() {
    document.getElementById('readerContainer').style.display = 'none';
    document.querySelector('.content-section').style.display = 'block';
}

/**
 * Altera tamanho da fonte no leitor
 * @param {number} delta - Variação do tamanho (-1 ou +1)
 */
function changeTextSize(delta) {
    currentFontSize += delta * 2;
    currentFontSize = Math.max(12, Math.min(24, currentFontSize));
    
    const content = document.querySelector('#readerContent > div:last-child');
    if (content) {
        content.style.fontSize = currentFontSize + 'px';
    }
    
    showNotification(`Tamanho da fonte: ${currentFontSize}px`, 'info');
}

/**
 * Alterna modo noturno no leitor
 */
function toggleNightMode() {
    nightMode = !nightMode;
    const reader = document.getElementById('readerContainer');
    
    if (nightMode) {
        reader.style.background = '#2c3e50';
        reader.style.color = '#ecf0f1';
        showNotification('Modo noturno ativado', 'info');
    } else {
        reader.style.background = 'white';
        reader.style.color = '#333';
        showNotification('Modo claro ativado', 'info');
    }
}

// ===========================================
// SISTEMA DE DOWNLOAD
// ===========================================

/**
 * Simula download de livro
 * @param {number} bookId - ID do livro
 */
function downloadBook(bookId) {
    const book = books.find(b => b.id === bookId);
    if (!book) return;

    // Criar conteúdo do arquivo
    const content = `${book.title}\n\nAutor: ${book.author}\nCategoria: ${book.category}\n\nDescrição:\n${book.description}\n\nConteúdo:\n${book.content.replace(/<[^>]*>/g, '')}`;
    
    // Simular download
    const blob = new Blob([content], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${book.title.replace(/[^a-zA-Z0-9]/g, '_')}.txt`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    
    showNotification(`Download iniciado: "${book.title}"`, 'success');
}

// ===========================================
// UPLOAD DE LIVROS
// ===========================================

/**
 * Processa upload de novo livro
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
        alert(messages.errors.emptyFields);
        return;
    }

    if (file && file.size > appConfig.maxFileSize) {
        alert(messages.errors.fileTooBig);
        return;
    }

    if (file && !isValidFileFormat(file.name)) {
        alert(messages.errors.invalidFormat);
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
    
    showNotification(`Livro "${title}" enviado com sucesso! Obrigado por contribuir, ${currentUser.name}!`, 'success');
}

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
 * Gera conteúdo de exemplo para livro enviado
 * @param {string} title - Título do livro
 * @param {string} author - Autor do livro
 * @param {string} description - Descrição do livro
 * @returns {string} Conteúdo HTML
 */
function generateSampleContent(title, author, description) {
    return `
        <h3>Prefácio</h3>
        <p>Este é o conteúdo do livro "${title}" por ${author}.</p>
        
        <h4>Sobre a Obra:</h4>
        <p>${description}</p>
        
        <h3>Capítulo 1</h3>
        <p>Conteúdo do primeiro capítulo será exibido aqui após o processamento do arquivo enviado.</p>
        
        <p><em>Nota: Este é um conteúdo de exemplo. Em um sistema real, o conteúdo seria extraído do arquivo enviado.</em></p>
    `;
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

// ===========================================
// PERFIL DO USUÁRIO
// ===========================================

/**
 * Atualiza dados do perfil na interface
 */
function updateProfileData() {
    if (!currentUser) return;

    // Atualizar informações básicas
    document.getElementById('profileName').textContent = currentUser.name;
    document.getElementById('profileEmail').textContent = currentUser.email;
    document.getElementById('profileJoinDate').textContent = `Membro desde: ${currentUser.joinDate}`;
    
    // Atualizar iniciais do avatar
    const initials = currentUser.name.split(' ').map(n => n[0]).join('').toUpperCase().substring(0, 2);
    document.getElementById('avatarInitials').textContent = initials;

    // Atualizar estatísticas
    document.getElementById('userBooksRead').textContent = formatNumber(userStats.booksRead);
    document.getElementById('userFavorites').textContent = formatNumber(favorites.size);
    document.getElementById('userContributions').textContent = formatNumber(userStats.contributions);
    document.getElementById('userReadingTime').textContent = userStats.readingTime + 'h';

    // Atualizar progresso da meta
    updateGoalProgress();
}

/**
 * Renderiza atividades do perfil
 */
function renderProfileActivity() {
    if (!currentUser) return;

    // Renderizar leituras recentes
    const recentReads = document.getElementById('recentReads');
    const recentBooks = books.filter(book => currentlyReading.has(book.id)).slice(0, 3);
    
    if (recentBooks.length > 0) {
        recentReads.innerHTML = recentBooks.map(book => `
            <div class="recent-item">
                <strong>${sanitizeString(book.title)}</strong> por ${sanitizeString(book.author)}
                <div style="font-size: 0.8rem; color: #666; margin-top: 5px;">
                    Categoria: ${sanitizeString(book.category)} • ${calculateReadingTime(book.content)} min
                </div>
            </div>
        `).join('');
    } else {
        recentReads.innerHTML = '<p>Nenhuma leitura recente</p>';
    }

    // Renderizar contribuições do usuário
    const contributionsList = document.getElementById('userContributionsList');
    const userBooks = books.filter(book => book.uploadedBy === currentUser.name);
    
    if (userBooks.length > 0) {
        contributionsList.innerHTML = userBooks.map(book => `
            <div class="recent-item">
                <strong>${sanitizeString(book.title)}</strong> por ${sanitizeString(book.author)}
                <div style="font-size: 0.8rem; color: #666; margin-top: 5px;">
                    Categoria: ${sanitizeString(book.category)} • Enviado em ${book.uploadDate || 'Data não disponível'}
                </div>
            </div>
        `).join('');
    } else {
        contributionsList.innerHTML = '<p>Nenhuma contribuição ainda</p>';
    }
}

/**
 * Atualiza meta de leitura
 */
function updateGoal() {
    const newGoal = parseInt(document.getElementById('yearlyGoal').value);
    if (newGoal > 0 && newGoal <= 365) {
        userStats.yearlyGoal = newGoal;
        updateGoalProgress();
        showNotification('Meta de leitura atualizada!', 'success');
    } else {
        alert('Por favor, digite uma meta válida (1-365 livros).');
    }
}

/**
 * Atualiza barra de progresso da meta
 */
function updateGoalProgress() {
    const progress = Math.min((userStats.booksRead / userStats.yearlyGoal) * 100, 100);
    document.getElementById('goalProgress').style.width = progress + '%';
    document.getElementById('goalText').textContent = 
        `${userStats.booksRead} de ${userStats.yearlyGoal} livros (${Math.round(progress)}%)`;
}

// ===========================================
// ESTATÍSTICAS GLOBAIS
// ===========================================

/**
 * Atualiza estatísticas globais da biblioteca
 */
function updateStats() {
    document.getElementById('totalBooks').textContent = formatNumber(books.length);
    
    // Animar números para efeito visual
    animateNumber('totalReads', 15892 + Math.floor(Math.random() * 1000));
    animateNumber('activeUsers', 3456 + Math.floor(Math.random() * 500));
    
    // Atualizar contagem de categorias
    const uniqueCategories = [...new Set(books.map(book => book.category))];
    document.getElementById('categoriesCount').textContent = uniqueCategories.length;
}

/**
 * Anima números nas estatísticas
 * @param {string} elementId - ID do elemento
 * @param {number} targetNumber - Número final
 */
function animateNumber(elementId, targetNumber) {
    const element = document.getElementById(elementId);
    const startNumber = parseInt(element.textContent.replace(/[.,]/g, ''));
    const duration = 1000;
    const stepTime = 20;
    const steps = duration / stepTime;
    const stepValue = (targetNumber - startNumber) / steps;
    let currentNumber = startNumber;

    const timer = setInterval(() => {
        currentNumber += stepValue;
        if ((stepValue > 0 && currentNumber >= targetNumber) || (stepValue < 0 && currentNumber <= targetNumber)) {
            currentNumber = targetNumber;
            clearInterval(timer);
        }
        element.textContent = formatNumber(Math.floor(currentNumber));
    }, stepTime);
}

// ===========================================
// SISTEMA DE NOTIFICAÇÕES
// ===========================================

/**
 * Exibe notificação temporária
 * @param {string} message - Mensagem da notificação
 * @param {string} type - Tipo da notificação ('success', 'error', 'info', 'warning')
 */
function showNotification(message, type = 'info') {
    // Remover notificação existente se houver
    const existingNotification = document.querySelector('.notification');
    if (existingNotification) {
        existingNotification.remove();
    }

    // Criar nova notificação
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${getNotificationColor(type)};
        color: white;
        padding: 15px 20px;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.3);
        z-index: 9999;
        max-width: 350px;
        font-weight: 500;
        animation: slideInRight 0.3s ease;
    `;
    notification.textContent = message;

    // Adicionar ao DOM
    document.body.appendChild(notification);

    // Remover automaticamente após 3 segundos
    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }, 3000);
}

/**
 * Retorna cor da notificação baseada no tipo
 * @param {string} type - Tipo da notificação
 * @returns {string} Cor CSS
 */
function getNotificationColor(type) {
    const colors = {
        success: '#28a745',
        error: '#dc3545',
        warning: '#ffc107',
        info: '#17a2b8'
    };
    return colors[type] || colors.info;
}

// ===========================================
// UTILITÁRIOS DE INTERFACE
// ===========================================

/**
 * Adiciona animações CSS dinamicamente
 */
function addDynamicStyles() {
    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideInRight {
            from { transform: translateX(100%); opacity: 0; }
            to { transform: translateX(0); opacity: 1; }
        }
        
        @keyframes slideOutRight {
            from { transform: translateX(0); opacity: 1; }
            to { transform: translateX(100%); opacity: 0; }
        }
        
        .book-card {
            transition: all 0.3s ease;
        }
        
        .book-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 10px 25px rgba(0,0,0,0.15);
        }
    `;
    document.head.appendChild(style);
}