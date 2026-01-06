import bcrypt from 'bcrypt';
import { prisma } from '@config/database';
import { logger } from '@config/logger';
import { generateToken } from '@middleware/auth';

const SALT_ROUNDS = 12;

export interface RegisterInput {
  email: string;
  username: string;
  password: string;
}

export interface LoginInput {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  player: {
    id: string;
    email: string;
    username: string;
    subscriptionTier: string;
    createdAt: Date;
  };
}

export class AuthService {
  async hashPassword(password: string): Promise<string> {
    return bcrypt.hash(password, SALT_ROUNDS);
  }

  async verifyPassword(password: string, hash: string): Promise<boolean> {
    return bcrypt.compare(password, hash);
  }

  async register(input: RegisterInput): Promise<AuthResponse> {
    const { email, username, password } = input;

    const existingEmail = await prisma.player.findUnique({
      where: { email },
    });
    if (existingEmail) {
      throw new Error('Email already registered');
    }

    const existingUsername = await prisma.player.findUnique({
      where: { username },
    });
    if (existingUsername) {
      throw new Error('Username already taken');
    }

    const passwordHash = await this.hashPassword(password);

    const player = await prisma.player.create({
      data: {
        email,
        username,
        passwordHash,
        profile: {
          create: {
            age: 18,
            health: 100,
            happiness: 100,
            wealth: 0,
            intelligence: 50,
            charisma: 50,
            physical: 50,
            creativity: 50,
          },
        },
        leaderboardEntry: {
          create: {
            score: 0,
            season: 'current',
          },
        },
      },
      include: {
        profile: true,
      },
    });

    const token = generateToken({
      playerId: player.id,
      email: player.email,
      username: player.username,
      subscriptionTier: player.subscriptionTier,
    });

    logger.info(`New player registered: ${player.id}`);

    return {
      token,
      player: {
        id: player.id,
        email: player.email,
        username: player.username,
        subscriptionTier: player.subscriptionTier,
        createdAt: player.createdAt,
      },
    };
  }

  async login(input: LoginInput): Promise<AuthResponse> {
    const { email, password } = input;

    const player = await prisma.player.findUnique({
      where: { email },
    });

    if (!player) {
      throw new Error('Invalid email or password');
    }

    if (player.status !== 'ACTIVE') {
      throw new Error('Account is suspended or banned');
    }

    const isValid = await this.verifyPassword(password, player.passwordHash);
    if (!isValid) {
      throw new Error('Invalid email or password');
    }

    await prisma.player.update({
      where: { id: player.id },
      data: { lastLoginAt: new Date() },
    });

    const token = generateToken({
      playerId: player.id,
      email: player.email,
      username: player.username,
      subscriptionTier: player.subscriptionTier,
    });

    logger.info(`Player logged in: ${player.id}`);

    return {
      token,
      player: {
        id: player.id,
        email: player.email,
        username: player.username,
        subscriptionTier: player.subscriptionTier,
        createdAt: player.createdAt,
      },
    };
  }

  async refreshToken(playerId: string): Promise<{ token: string }> {
    const player = await prisma.player.findUnique({
      where: { id: playerId },
    });

    if (!player) {
      throw new Error('Player not found');
    }

    if (player.status !== 'ACTIVE') {
      throw new Error('Account is suspended or banned');
    }

    const token = generateToken({
      playerId: player.id,
      email: player.email,
      username: player.username,
      subscriptionTier: player.subscriptionTier,
    });

    return { token };
  }

  async changePassword(
    playerId: string,
    currentPassword: string,
    newPassword: string
  ): Promise<void> {
    const player = await prisma.player.findUnique({
      where: { id: playerId },
    });

    if (!player) {
      throw new Error('Player not found');
    }

    const isValid = await this.verifyPassword(currentPassword, player.passwordHash);
    if (!isValid) {
      throw new Error('Current password is incorrect');
    }

    const newPasswordHash = await this.hashPassword(newPassword);

    await prisma.player.update({
      where: { id: playerId },
      data: { passwordHash: newPasswordHash },
    });

    logger.info(`Password changed for player: ${playerId}`);
  }
}

export const authService = new AuthService();
