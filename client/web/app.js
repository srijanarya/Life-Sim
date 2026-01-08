// LifeCraft Web UI - Game Logic

const API_BASE = 'http://localhost:3000/api';

// Game State
let state = {
    token: localStorage.getItem('lifecraft_token'),
    player: JSON.parse(localStorage.getItem('lifecraft_player') || 'null'),
    profile: null,
    game: null,
    currentEvent: null
};

// API Helper
async function api(endpoint, options = {}) {
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };

    if (state.token) {
        headers['Authorization'] = `Bearer ${state.token}`;
    }

    try {
        const response = await fetch(`${API_BASE}${endpoint}`, {
            ...options,
            headers
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.error || 'Request failed');
        }

        return data;
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
}

// Screen Management
function showScreen(screenId) {
    document.querySelectorAll('.screen').forEach(s => s.classList.remove('active'));
    document.getElementById(screenId).classList.add('active');
}

// Toast Notifications
function showToast(message, type = 'success') {
    const toast = document.getElementById('toast');
    toast.textContent = message;
    toast.className = `toast show ${type}`;

    setTimeout(() => {
        toast.classList.remove('show');
    }, 3000);
}

// Auth Functions
function showLogin() {
    document.getElementById('login-form').classList.remove('hidden');
    document.getElementById('register-form').classList.add('hidden');
}

function showRegister() {
    document.getElementById('login-form').classList.add('hidden');
    document.getElementById('register-form').classList.remove('hidden');
}

async function login() {
    const email = document.getElementById('login-email').value;
    const password = document.getElementById('login-password').value;

    if (!email || !password) {
        showToast('Please fill in all fields', 'error');
        return;
    }

    try {
        const data = await api('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });

        state.token = data.token;
        state.player = data.player;
        localStorage.setItem('lifecraft_token', data.token);
        localStorage.setItem('lifecraft_player', JSON.stringify(data.player));

        showToast('Welcome back!');
        goToMenu();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

async function register() {
    const username = document.getElementById('register-username').value;
    const email = document.getElementById('register-email').value;
    const password = document.getElementById('register-password').value;

    if (!username || !email || !password) {
        showToast('Please fill in all fields', 'error');
        return;
    }

    try {
        const data = await api('/auth/register', {
            method: 'POST',
            body: JSON.stringify({ username, email, password })
        });

        state.token = data.token;
        state.player = data.player;
        localStorage.setItem('lifecraft_token', data.token);
        localStorage.setItem('lifecraft_player', JSON.stringify(data.player));

        showToast('Account created!');
        goToMenu();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

function logout() {
    state.token = null;
    state.player = null;
    state.profile = null;
    state.game = null;
    localStorage.removeItem('lifecraft_token');
    localStorage.removeItem('lifecraft_player');
    showScreen('auth-screen');
}

// Menu Functions
async function goToMenu() {
    if (!state.player) {
        showScreen('auth-screen');
        return;
    }

    document.getElementById('player-name').textContent = state.player.username || 'Player';

    // Check for existing game
    try {
        const games = await api(`/game?playerId=${state.player.id}`);
        if (games && games.length > 0) {
            document.getElementById('continue-btn').disabled = false;
            state.game = games[0];
        }
    } catch (error) {
        console.log('No existing game found');
    }

    showScreen('menu-screen');
}

// Character Creation
function newGame() {
    // Reset creation form
    document.getElementById('char-name').value = state.player?.username || '';
    document.getElementById('char-age').value = 18;
    document.querySelectorAll('.stat-sliders input[type="range"]').forEach(input => {
        input.value = 0;
    });
    updatePoints();
    showScreen('creation-screen');
}

function updatePoints() {
    const stats = ['health', 'intelligence', 'charisma', 'creativity', 'happiness'];
    let used = 0;

    stats.forEach(stat => {
        const input = document.getElementById(`stat-${stat}`);
        const value = parseInt(input.value);
        used += value;

        // Update display value
        const display = input.parentElement.querySelector('.stat-value');
        display.textContent = 50 + value;
    });

    const remaining = 20 - used;
    document.getElementById('points-remaining').textContent = remaining;

    // Prevent over-allocation
    if (remaining < 0) {
        stats.forEach(stat => {
            const input = document.getElementById(`stat-${stat}`);
            if (parseInt(input.value) > 0) {
                input.value = Math.max(0, parseInt(input.value) + remaining);
                const display = input.parentElement.querySelector('.stat-value');
                display.textContent = 50 + parseInt(input.value);
            }
        });
        document.getElementById('points-remaining').textContent = 0;
    }
}

async function startGame() {
    const name = document.getElementById('char-name').value || 'Unknown';
    const age = parseInt(document.getElementById('char-age').value) || 18;

    const stats = {
        health: 50 + parseInt(document.getElementById('stat-health').value),
        intelligence: 50 + parseInt(document.getElementById('stat-intelligence').value),
        charisma: 50 + parseInt(document.getElementById('stat-charisma').value),
        creativity: 50 + parseInt(document.getElementById('stat-creativity').value),
        happiness: 50 + parseInt(document.getElementById('stat-happiness').value),
        wealth: 0
    };

    try {
        // Create or update profile
        const profile = await api(`/player/${state.player.id}/profile`, {
            method: 'PATCH',
            body: JSON.stringify({
                displayName: name,
                ...stats
            })
        });
        state.profile = profile;

        // Create game
        const game = await api('/game', {
            method: 'POST',
            body: JSON.stringify({
                playerId: state.player.id,
                initialTraits: { startingAge: age }
            })
        });
        state.game = game;

        showToast('Your life begins!');
        enterGameplay();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

async function continueGame() {
    if (!state.game) {
        showToast('No saved game found', 'error');
        return;
    }

    try {
        // Load profile
        const profile = await api(`/player/${state.player.id}/profile`);
        state.profile = profile;

        enterGameplay();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

// Gameplay
function enterGameplay() {
    updateGameUI();
    showScreen('game-screen');

    // Try to get an event
    getRandomEvent();
}

function updateGameUI() {
    if (!state.profile || !state.game) return;

    // Update stats bars
    const health = state.profile.health || 50;
    const happiness = state.profile.happiness || 50;
    const wealth = state.profile.wealth || 0;

    document.getElementById('health-bar').style.width = `${health}%`;
    document.getElementById('health-value').textContent = health;
    document.getElementById('happiness-bar').style.width = `${happiness}%`;
    document.getElementById('happiness-value').textContent = happiness;
    document.getElementById('wealth-value').textContent = `$${wealth.toLocaleString()}`;

    // Update character info
    document.getElementById('game-char-name').textContent = state.profile.displayName || 'Unknown';
    document.getElementById('game-age').textContent = state.game.currentAge || 18;
    document.getElementById('game-year').textContent = state.game.currentYear || 1;
    document.getElementById('game-month').textContent = state.game.currentMonth || 1;
}

async function getRandomEvent() {
    try {
        const data = await api('/events/random');

        if (data && data.event) {
            state.currentEvent = data.event;
            displayEvent(data.event);
        } else {
            showNoEvent();
        }
    } catch (error) {
        console.log('No event available');
        showNoEvent();
    }
}

function displayEvent(event) {
    document.getElementById('event-container').style.display = 'block';
    document.getElementById('no-event').style.display = 'none';

    document.getElementById('event-type').textContent = event.eventType || 'Life Event';
    document.getElementById('event-rarity').textContent = event.rarity || 'Common';
    document.getElementById('event-rarity').className = `event-rarity ${(event.rarity || '').toLowerCase()}`;
    document.getElementById('event-title').textContent = event.title || 'Something happens...';
    document.getElementById('event-description').textContent = event.description || '';

    // Load decisions
    loadDecisions(event.id);
}

async function loadDecisions(eventId) {
    const container = document.getElementById('decisions-container');
    container.innerHTML = '<p style="color: var(--text-secondary);">Loading choices...</p>';

    try {
        const decisions = await api(`/events/templates/${eventId}/decisions`);

        container.innerHTML = '';

        if (decisions && decisions.length > 0) {
            decisions.forEach(decision => {
                const btn = document.createElement('button');
                btn.className = 'decision-btn';
                btn.textContent = decision.text || decision.description || 'Choose this option';
                btn.onclick = () => makeDecision(decision.id);
                container.appendChild(btn);
            });
        } else {
            container.innerHTML = '<button class="decision-btn" onclick="advanceTime()">Continue...</button>';
        }
    } catch (error) {
        container.innerHTML = '<button class="decision-btn" onclick="advanceTime()">Continue...</button>';
    }
}

async function makeDecision(decisionId) {
    if (!state.game || !state.currentEvent) return;

    try {
        const result = await api(`/game/${state.game.id}/decisions`, {
            method: 'POST',
            body: JSON.stringify({
                eventId: state.currentEvent.id,
                decisionId: decisionId
            })
        });

        // Refresh profile to get updated stats
        state.profile = await api(`/player/${state.player.id}/profile`);

        showToast('Choice made!');
        updateGameUI();

        // Clear event and advance
        state.currentEvent = null;
        advanceTime();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

function showNoEvent() {
    document.getElementById('event-container').style.display = 'none';
    document.getElementById('no-event').style.display = 'block';
}

async function advanceTime() {
    if (!state.game) return;

    try {
        const result = await api(`/game/${state.game.id}/advance`, {
            method: 'POST',
            body: JSON.stringify({ months: 1 })
        });

        state.game = result.gameState || result;

        // Check if there's an event
        if (result.event || result.eventTemplate) {
            state.currentEvent = result.eventTemplate || result.event;
            displayEvent(state.currentEvent);
        } else {
            // Try to get a random event
            await getRandomEvent();
        }

        // Refresh profile
        state.profile = await api(`/player/${state.player.id}/profile`);
        updateGameUI();

    } catch (error) {
        if (error.message.includes('cooldown')) {
            showToast('Please wait a moment...', 'error');
        } else {
            showToast(error.message, 'error');
        }
    }
}

function viewStats() {
    if (!state.profile) return;

    const container = document.getElementById('full-stats');
    const stats = [
        { name: 'Health', value: state.profile.health || 50, color: 'var(--health-color)' },
        { name: 'Happiness', value: state.profile.happiness || 50, color: 'var(--happiness-color)' },
        { name: 'Intelligence', value: state.profile.intelligence || 50, color: 'var(--intelligence-color)' },
        { name: 'Charisma', value: state.profile.charisma || 50, color: 'var(--charisma-color)' },
        { name: 'Creativity', value: state.profile.creativity || 50, color: 'var(--creativity-color)' },
        { name: 'Wealth', value: `$${(state.profile.wealth || 0).toLocaleString()}`, color: 'var(--wealth-color)' }
    ];

    container.innerHTML = stats.map(stat => `
        <div class="full-stat-row">
            <span class="full-stat-name">${stat.name}</span>
            <span class="full-stat-value" style="color: ${stat.color}">${stat.value}</span>
        </div>
    `).join('');

    document.getElementById('stats-modal').classList.add('active');
}

function closeStats() {
    document.getElementById('stats-modal').classList.remove('active');
}

function quitGame() {
    if (confirm('Are you sure you want to quit? Your progress is saved.')) {
        state.game = null;
        state.currentEvent = null;
        goToMenu();
    }
}

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    if (state.token && state.player) {
        goToMenu();
    } else {
        showScreen('auth-screen');
    }
});

// Handle Enter key for forms
document.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') {
        if (document.getElementById('auth-screen').classList.contains('active')) {
            if (!document.getElementById('login-form').classList.contains('hidden')) {
                login();
            } else {
                register();
            }
        }
    }
});
