import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

async function main() {
  console.log('Starting database seed...');

  const career = await prisma.career.create({
    data: {
      name: 'Software Engineer',
      description: 'Develop and maintain software applications',
      type: 'TECH',
      minIntelligence: 70,
      minCreativity: 50,
      minCharisma: 40,
      baseSalary: 60000,
      maxSalary: 200000,
      promotionYears: 2,
    },
  });
  console.log(`Created career: ${career.name}`);

  const eventTemplate = await prisma.eventTemplate.create({
    data: {
      title: 'Job Interview',
      description: 'You have an important job interview today',
      eventType: 'CAREER_EVENT',
      rarity: 'COMMON',
      minAge: 18,
      maxAge: 65,
      requiredCareer: null,
      decisions: {
        create: [
          {
            text: 'Prepare thoroughly',
            order: 1,
            outcomes: JSON.stringify({
              intelligenceBoost: 5,
              charismaBoost: 3,
              successChance: 0.8,
            }),
          },
          {
            text: 'Wing it',
            order: 2,
            outcomes: JSON.stringify({
              charismaBoost: 2,
              intelligencePenalty: 2,
              successChance: 0.4,
            }),
          },
        ],
      },
    },
  });
  console.log(`Created event template: ${eventTemplate.title}`);

  const achievement = await prisma.achievement.create({
    data: {
      name: 'First Million',
      description: 'Accumulate 1,000,000 in wealth',
      type: 'WEALTH',
      rewardXp: 1000,
      rewardCurrency: 500,
    },
  });
  console.log(`Created achievement: ${achievement.name}`);

  console.log('Database seed completed successfully!');
}

main()
  .catch((error) => {
    console.error('Error seeding database:', error);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
