using System;
using System.Linq;

namespace ConsoleApp3
{

    public enum Rarity { Обычное, Редкое, Эпическое }
    public enum PlayerClassEnum { Воин, Маг, Лучник }
    public enum PlayerRaceEnum { Человек, Эльф, Дварф, Орк, Тифлинг }
    public enum EnemyRaceEnum { Гоблин, РазумныйГриб, Огр, Мимик }
    public enum EnemyTypeSimple { Воин, Маг, Лучник, Берсерк }
    public enum WeaponCategory { Меч, Лук, Посох, Кинжал, Кулак }
    public enum ArmorCategory { Лёгкая, Средняя, Тяжёлая, Мантия }

    public static class ClassBonuses
    {
        public const int WeaponAffinityBonus = 2; // доп урон если оружие подходит классу
        public const int ArmorAffinityBonus = 1;  // доп КБ если броня подходит классу
    }

    // Генератор случайных чисел
    static class Utils
    {
        public static Random rnd = new Random();
    }


    class Program
    {
        static void Main()
        {
            // Ввод имени игрока, выбор расы и класса
            Console.Write("Введите имя персонажа: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) name = "Герой";

            var race = ChooseEnum<PlayerRaceEnum>("Выберите расу:");
            var pclass = ChooseEnum<PlayerClassEnum>("Выберите класс:");
            var player = CreatePlayerWithStartEquipment(name, race, pclass);

            Console.WriteLine($"Создан: {player.Name} ({player.Race}/{player.PlayerClass})");
            player.ShowStats();

            // Главный цикл: меню действий игрока
            bool exit = false;
            while (!exit && player.IsAlive)
            {
                Console.WriteLine("\n--- Главное меню ---");
                Console.WriteLine("1) Показать характеристики");
                Console.WriteLine("2) Показать инвентарь");
                Console.WriteLine("3) Экипировать предмет");
                Console.WriteLine("4) Использовать предмет");
                Console.WriteLine("5) Исследовать комнату");
                Console.WriteLine("6) Выйти");
                Console.Write("Выберите: ");
                var c = Console.ReadLine();
                switch (c)
                {
                    case "1": player.ShowStats(); break;
                    case "2": player.ShowInventory(); break;
                    case "3":
                        player.ShowInventory();
                        Console.Write("Индекс предмета для экипировки: ");
                        if (int.TryParse(Console.ReadLine(), out int idx3))
                        {
                            if (idx3 >= 0 && idx3 < player.Inventory.Count)
                            {
                                var it = player.Inventory[idx3];
                                try
                                {
                                    if (it is Weapon w) player.EquipWeapon(w);
                                    else if (it is Armor a) player.EquipArmor(a);
                                    else Console.WriteLine("Этот предмет нельзя экипировать.");
                                }
                                catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
                            }
                            else Console.WriteLine("Индекс вне диапазона.");
                        }
                        else Console.WriteLine("Неверный ввод.");
                        break;
                    case "4":
                        player.ShowInventory();
                        Console.Write("Индекс предмета для использования: ");
                        if (int.TryParse(Console.ReadLine(), out int idx4)) player.UseItem(idx4);
                        else Console.WriteLine("Неверный ввод.");
                        break;
                    case "5":
                        ExploreRoomInteractive(player); // основной сценарий исследования комнаты и боёв
                        break;
                    case "6": exit = true; break;
                    default: Console.WriteLine("Неверный выбор."); break;
                }
            }

            // Завершение игры
            if (!player.IsAlive) Console.WriteLine("Вы погибли. Игра окончена.");
            else Console.WriteLine("Вы вышли из игры. До скорых встреч!");
        }

        // Функция выбора элемента enum (показывает варианты и возвращает выбранный)
        static T ChooseEnum<T>(string prompt) where T : Enum
        {
            Console.WriteLine(prompt);
            var vals = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            for (int i = 0; i < vals.Length; i++) Console.WriteLine($"{i}) {vals[i]}");
            Console.Write("Ваш выбор (число): ");
            if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 0 && idx < vals.Length) return vals[idx];
            Console.WriteLine("Неверный ввод — выбран вариант 0.");
            return vals[0];
        }

        // Создаёт игрока и даёт стартовую экипировку обычного качества
        static Player CreatePlayerWithStartEquipment(string name, PlayerRaceEnum race, PlayerClassEnum pclass)
        {
            var player = new Player(name, race, pclass);

            // Выбор обычного оружия/брони для класса, если есть
            Weapon GetCommonWeaponForClass(PlayerClassEnum pc)
            {
                var byAffinity = ItemDatabase.Weapons.FirstOrDefault(w => w.Rarity == Rarity.Обычное && w.Affinity == pc);
                if (byAffinity != null) return byAffinity;
                return ItemDatabase.Weapons.FirstOrDefault(w => w.Rarity == Rarity.Обычное);
            }

            Armor GetCommonArmorForClass(PlayerClassEnum pc)
            {
                var byAffinity = ItemDatabase.Armors.FirstOrDefault(a => a.Rarity == Rarity.Обычное && a.Affinity == pc);
                if (byAffinity != null) return byAffinity;
                return ItemDatabase.Armors.FirstOrDefault(a => a.Rarity == Rarity.Обычное);
            }

            var starterWeapon = GetCommonWeaponForClass(pclass);
            var starterArmor = GetCommonArmorForClass(pclass);

            if (starterWeapon != null) { player.AddToInventory(starterWeapon); player.EquipWeapon(starterWeapon); }
            if (starterArmor != null) { player.AddToInventory(starterArmor); player.EquipArmor(starterArmor); }

            // Добавляем зелья 
            player.AddToInventory(ItemDatabase.Potions.First(p => p.Name == "Зелье лечения"));
            player.AddToInventory(ItemDatabase.Potions.First(p => p.Name == "Зелье маны"));

            // Стартовый обычный артефакт 
            var starterArtifact = ItemDatabase.Artifacts.FirstOrDefault(a => a.Rarity == Rarity.Обычное);
            if (starterArtifact != null) player.AddToInventory(starterArtifact);

            // Для мага даём стартовую ману 
            if (pclass == PlayerClassEnum.Маг) { player.MaxMana += 10; player.Mana += 10; }

            player.CalculateStats();
            return player;
        }

        // Исследование комнаты: либо сундук, либо встреча с врагом
        static void ExploreRoomInteractive(Player hero)
        {
            Console.WriteLine("\nВы исследуете комнату...");
            double r = Utils.rnd.NextDouble();
            if (r < GameGenerator.ChestChance)
            {
                // Сундук или мимик
                var (items, isMimic) = GameGenerator.GenerateChest();
                if (isMimic)
                {
                    // Мимик - ловушка
                    Console.WriteLine("Сундук оказался мимиком! Он атакует!");
                    var mimic = new Enemy("Мимик", EnemyRaceEnum.Мимик, EnemyTypeSimple.Воин);
                    while (hero.IsAlive && mimic.IsAlive)
                    {
                        Console.WriteLine($"\n--- Битва с {mimic.Name} ---");
                        Console.WriteLine($"{mimic.Name} HP: {mimic.Health}/{mimic.MaxHealth}  КБ: {mimic.ArmorClass}");
                        Console.WriteLine("1) Атаковать  2) Использовать предмет  3) Попытаться бежать");
                        var pick = Console.ReadLine();
                        if (pick == "1") hero.PlayerAttack(mimic);
                        else if (pick == "2") { hero.ShowInventory(); if (int.TryParse(Console.ReadLine(), out int idx)) hero.UseItem(idx); }
                        else if (pick == "3")
                        {
                            if (Utils.rnd.NextDouble() < 0.5) Console.WriteLine("Убежать не удалось.");
                            else { Console.WriteLine("Убежали."); return; }
                        }
                        else Console.WriteLine("Неверный ввод.");

                        if (mimic.IsAlive) mimic.Attack(hero);
                    }

                    if (!mimic.IsAlive) Console.WriteLine("Мимик побеждён! Он прятал добычу.");
                    var drop = ItemDatabase.GetRandomItemWithRarities(Rarity.Редкое, Rarity.Эпическое);
                    if (drop != null) { Console.WriteLine($"В добыче: {drop}"); hero.AddToInventory(drop); }
                    return;
                }
                else
                {
                    // Обычный сундук
                    Console.WriteLine("Вы нашли сундук. Внутри:");
                    foreach (var it in items) { Console.WriteLine($" - {it}"); hero.AddToInventory(it); }
                    return;
                }
            }
            else
            {
                // Генерация врага и вход в бой
                var enemy = GameGenerator.GenerateEnemy();
                Console.WriteLine($"В комнате появился враг: {enemy}");
                while (hero.IsAlive && enemy.IsAlive)
                {
                    Console.WriteLine($"\n--- Битва с {enemy.Name} ---");
                    Console.WriteLine($"{enemy.Name} HP: {enemy.Health}/{enemy.MaxHealth}  КБ: {enemy.ArmorClass}");
                    Console.WriteLine("1) Атаковать");
                    if (hero.PlayerClass == PlayerClassEnum.Маг) Console.WriteLine("2) Заклинания");
                    Console.WriteLine("3) Использовать предмет");
                    Console.WriteLine("4) Попытаться бежать");
                    Console.Write("Выбор: ");
                    var action = Console.ReadLine();
                    if (action == "1") hero.PlayerAttack(enemy);
                    else if (action == "2" && hero.PlayerClass == PlayerClassEnum.Маг)
                    {
                        Console.WriteLine("a) Огненный шар (8 маны)");
                        Console.WriteLine("b) Исцеление (6 маны)");
                        Console.WriteLine("c) Ледяной луч (5 маны)");
                        var s = Console.ReadLine();
                        try
                        {
                            if (s == "a") hero.CastFireball(enemy);
                            else if (s == "b") hero.CastHeal();
                            else if (s == "c") hero.CastFreezingRay(enemy);
                            else Console.WriteLine("Неверный выбор.");
                        }
                        catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
                    }
                    else if (action == "3") { hero.ShowInventory(); Console.Write("Индекс: "); if (int.TryParse(Console.ReadLine(), out int idx)) hero.UseItem(idx); }
                    else if ((action == "4") || (action == "2" && hero.PlayerClass != PlayerClassEnum.Маг))
                    {
                        if (Utils.rnd.NextDouble() < 0.5) Console.WriteLine("Ты попытался сбежать, но споткнулся и упал.");
                        else { Console.WriteLine("Убежали."); return; }
                    }
                    else Console.WriteLine("Неверный ввод.");

                    // Ход врага 
                    if (enemy.IsAlive) enemy.Attack(hero);
                }

                // После боя выдаём лут и сообщаем результат
                if (!enemy.IsAlive)
                {
                    Console.WriteLine($"Вы победили {enemy.Name}!");
                    var loot = GameGenerator.GenerateEnemyLoot();
                    Console.WriteLine($"В добыче: {loot}");
                    if (loot != null) hero.AddToInventory(loot);
                }
                else if (!hero.IsAlive) Console.WriteLine("Вы погибли в бою...");
                return;
            }
        }
    }
}
// коментарий
//абвглд

//валид инвалид

// ===== CHANGE FROM branch-B: CONFLICT-B =====
// ===== CHANGE FROM branch-A: CONFLICT-A =====