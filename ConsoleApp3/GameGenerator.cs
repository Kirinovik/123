using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    // Логика появления врагов и сундуков
    static class GameGenerator
    {
        // Шанс, что сундук мимик, и шанс появления обычного сундука
        public static double ChestMimicChance = 0.30;
        public static double ChestChance = 0.30;

        // Генерация обычного врага 
        public static Enemy GenerateEnemy()
        {
            var races = new EnemyRaceEnum[] { EnemyRaceEnum.Гоблин, EnemyRaceEnum.РазумныйГриб, EnemyRaceEnum.Огр };
            var r = races[Utils.rnd.Next(races.Length)];

            if (r == EnemyRaceEnum.РазумныйГриб) return new Enemy("Разумный Гриб", r, EnemyTypeSimple.Воин);
            if (r == EnemyRaceEnum.Огр) return new Enemy("Огр", r, EnemyTypeSimple.Воин);
            // Для гоблинов выбираем тип (воин/маг/лучник/берсерк)
            var types = Enum.GetValues(typeof(EnemyTypeSimple)).Cast<EnemyTypeSimple>().ToArray();
            var t = types[Utils.rnd.Next(types.Length)];
            return new Enemy($"Гоблин {t}", EnemyRaceEnum.Гоблин, t);
        }

        // Генерация сундука: либо мимик, либо набор предметов
        public static (Item[] items, bool isMimic) GenerateChest()
        {
            bool isMimic = Utils.rnd.NextDouble() < ChestMimicChance;
            if (isMimic) return (Array.Empty<Item>(), true);

            int count = Utils.rnd.Next(1, 4); // 1..3 предмета
            var list = new List<Item>();
            for (int i = 0; i < count; i++)
            {
                if (Utils.rnd.NextDouble() < 0.4)
                    list.Add(ItemDatabase.GetRandomItemWithRarities(Rarity.Редкое));
                else
                    list.Add(ItemDatabase.GetRandomItemWithRarities(Rarity.Эпическое));
            }
            return (list.ToArray(), false);
        }

        // Добыча с врага
        public static Item GenerateEnemyLoot()
        {
            double r = Utils.rnd.NextDouble();
            if (r < 0.6) return ItemDatabase.GetRandomItemWithRarities(Rarity.Обычное);
            return ItemDatabase.GetRandomItemWithRarities(Rarity.Редкое);
        }
    }
}
