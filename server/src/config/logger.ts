import winston from 'winston';
import { config } from './index';

const logFormat = winston.format.combine(
  winston.format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss' }),
  winston.format.errors({ stack: true }),
  winston.format.splat(),
  winston.format.json()
);

const consoleFormat = winston.format.combine(
  winston.format.colorize(),
  winston.format.timestamp({ format: 'HH:mm:ss' }),
  winston.format.printf(({ timestamp, level, message, ...meta }) => {
    let msg = `${timestamp} [${level}]: ${message}`;
    if (Object.keys(meta).length > 0) {
      msg += `\n${JSON.stringify(meta, null, 2)}`;
    }
    return msg;
  })
);

export const logger = winston.createLogger({
  level: config.isDev ? 'debug' : 'info',
  format: logFormat,
  defaultMeta: { service: 'lifecraft-server' },
  transports: [
    new winston.transports.Console({
      format: consoleFormat,
    }),
  ],
});

if (config.isProd) {
  const TEN_MB_IN_BYTES = 10 * 1024 * 1024;

  logger.add(
    new winston.transports.File({
      filename: 'logs/error.log',
      level: 'error',
      maxsize: TEN_MB_IN_BYTES,
      maxFiles: 5,
    })
  );

  logger.add(
    new winston.transports.File({
      filename: 'logs/combined.log',
      maxsize: 10485760,
      maxFiles: 10,
    })
  );
}
