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

  

    public abstract class Item
    {
        public string Name { get; set; }
        public int Value { get; set; }      // цена
        public Rarity Rarity { get; set; } // редкость

        protected Item(string name, int value, Rarity rarity)
        {
            Name = name; Value = value; Rarity = rarity;
        }

        public virtual void Use(Player player) => Console.WriteLine($"{Name} нельзя использовать прямо сейчас.");
        public override string ToString() => $"{Name} [{Rarity}]";
    }

    public class Weapon : Item
    {
        public WeaponCategory Category { get; set; }
        public int DamageMin { get; set; }
        public int DamageMax { get; set; }
        public PlayerClassEnum? Affinity { get; set; } 

        public Weapon(string name, int value, Rarity rarity, WeaponCategory category, int dmgMin, int dmgMax, PlayerClassEnum? affinity = null)
            : base(name, value, rarity)
        {
            Category = category;
            DamageMin = dmgMin;
            DamageMax = dmgMax;
            Affinity = affinity;
        }

        public override string ToString() =>
            $"{Name} [{Rarity}] ({Category}) {DamageMin}-{DamageMax}" + (Affinity != null ? $" (Бонус для {Affinity})" : "");
    }

    public class Armor : Item
    {
        public ArmorCategory Category { get; set; }
        public int ArmorValue { get; set; }
        public PlayerClassEnum? Affinity { get; set; }

        public Armor(string name, int value, Rarity rarity, ArmorCategory category, int armorValue, PlayerClassEnum? affinity = null)
            : base(name, value, rarity)
        {
            Category = category;
            ArmorValue = armorValue;
            Affinity = affinity;
        }

        public override string ToString() =>
            $"{Name} [{Rarity}] ({Category}) +{ArmorValue} КБ" + (Affinity != null ? $" (Бонус для {Affinity})" : "");
    }

    public class Potion : Item
    {
        public int HealAmount { get; set; }
        public int ManaAmount { get; set; }

        public Potion(string name, int value, Rarity rarity, int healAmount = 0, int manaAmount = 0)
            : base(name, value, rarity)
        {
            HealAmount = healAmount;
            ManaAmount = manaAmount;
        }

        // При использовании зелей применяются эффекты и предмет удаляется из инвентаря
        public override void Use(Player player)
        {
            if (HealAmount > 0)
            {
                player.TakeHealing(HealAmount);
                Console.WriteLine($"{player.Name} использовал {Name} и восстановил {HealAmount} HP.");
            }
            else if (ManaAmount > 0)
            {
                player.RestoreMana(ManaAmount);
                Console.WriteLine($"{player.Name} использовал {Name} и восстановил {ManaAmount} MP.");
            }
        }

        public override string ToString()
        {
            if (HealAmount > 0) return $"{Name} [{Rarity}] Зелье +{HealAmount} HP";
            if (ManaAmount > 0) return $"{Name} [{Rarity}] Зелье +{ManaAmount} MP";
            return base.ToString();
        }
    }

    // Артефакты дают постоянный эффект 
    public class Artifact : Item
    {
        public string Description { get; set; }
        public Action<Player> Effect { get; set; }
        private bool applied = false; // флаг чтобы не применять эффект дважды

        public Artifact(string name, int value, Rarity rarity, string description, Action<Player> effect)
            : base(name, value, rarity)
        {
            Description = description;
            Effect = effect;
        }

        // При использовании артефакта эффект становится постоянным
        public override void Use(Player player)
        {
            if (applied)
            {
                Console.WriteLine($"Артефакт {Name} уже активирован — эффект постоянный и повторно не применяется.");
                return;
            }
            Console.WriteLine($"{player.Name} активирует артефакт {Name}: {Description}");
            Effect?.Invoke(player);
            applied = true;
        }

        public override string ToString() => $"{Name} [{Rarity}] Артефакт: {Description}";
    }

    
    public abstract class Character
    {
        // Имя, здоровье, максимум здоровье, класс брони, экипированное оружие/броня
        public string Name { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int ArmorClass { get; set; }   
        public Weapon EquippedWeapon { get; set; }
        public Armor EquippedArmor { get; set; }
        public bool IsAlive => Health > 0;
        protected static Random rnd = Utils.rnd;
        public Character(string name) => Name = name;

        // Получения урона
        public virtual void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health < 0) Health = 0;
            Console.WriteLine($"{Name} получил {amount} урона. HP: {Health}/{MaxHealth}");
        }

        // Лечение
        public virtual void TakeHealing(int amount)
        {
            Health += amount;
            if (Health > MaxHealth) Health = MaxHealth;
            Console.WriteLine($"{Name} восстановил {amount} HP. HP: {Health}/{MaxHealth}");
        }

        public abstract void CalculateStats();

        public override string ToString() =>
            $"{Name} HP:{Health}/{MaxHealth} КБ:{ArmorClass} Оружие:{EquippedWeapon?.Name ?? "Нет"} Броня:{EquippedArmor?.Name ?? "Нет"}";
    }

    
    public class Player : Character
    {
        public PlayerRaceEnum Race { get; set; }
        public PlayerClassEnum PlayerClass { get; set; }
        public List<Item> Inventory { get; set; } = new();
        public int Mana { get; set; }
        public int MaxMana { get; set; }

        // Постоянные накопленные бонусы от артефактов и прочего
        public int PermanentDamageBonus { get; set; } = 0;
        public int PermanentArmorBonus { get; set; } = 0;
        public int PermanentManaBonus { get; set; } = 0;
        public int CritDamageBonus { get; set; } = 0;
        public int EvasionBonus { get; set; } = 0;

        public Player(string name, PlayerRaceEnum race, PlayerClassEnum pclass) : base(name)
        {
            Race = race; PlayerClass = pclass;
            CalculateStats(); // вычисление стартовых характеристик
        }

    
        public override void CalculateStats()
        {
            MaxHealth = 50; // базовый HP
            ArmorClass = 10; // базовый КБ
            MaxMana = 0;     

            // Бонусы по классу
            switch (PlayerClass)
            {
                case PlayerClassEnum.Воин: MaxHealth += 25; break;
                case PlayerClassEnum.Маг: MaxMana = 40; break;
                case PlayerClassEnum.Лучник: MaxHealth += 10; ArmorClass += 1; break;
            }

              // Расовые бонусы
            switch (Race)
            {
                case PlayerRaceEnum.Человек:
                    MaxHealth += 4;             // Человек: +4 HP
                    break;
                case PlayerRaceEnum.Эльф:
                    MaxMana += 5;               // Эльф: +5 MP
                    break;
                case PlayerRaceEnum.Дварф:
                    MaxHealth += 2;             // Дварф: +2 HP
                    ArmorClass += 2;            // Дварф: +2 КБ
                    break;
                case PlayerRaceEnum.Орк:
                    MaxHealth += 7;             // Орк: +7 HP
                    break;
                case PlayerRaceEnum.Тифлинг:
                    MaxHealth += 2;             // Тифлинг: +2 HP
                    MaxMana += 3;               // Тифлинг: +3 MP
                    break;
            }

            if (EquippedArmor != null)
            {
                ArmorClass += EquippedArmor.ArmorValue;
                // Если броня подходит по классу, то будет дополнительный бонус 
                if (EquippedArmor.Affinity == PlayerClass) ArmorClass += ClassBonuses.ArmorAffinityBonus;
            }

            // Учёт постоянного бонуса
            MaxHealth += PermanentArmorBonus;
            ArmorClass += PermanentArmorBonus;

            // Инициализация текущих показателей
            if (Health == 0) Health = MaxHealth;
            if (PlayerClass == PlayerClassEnum.Маг && Mana == 0) Mana = MaxMana + PermanentManaBonus;
            // Для не-магических классов мана отсутствует
            if (PlayerClass != PlayerClassEnum.Маг) { Mana = 0; MaxMana = 0; }
        }

        // Экипировка оружия
        public void EquipWeapon(Weapon w)
        {
            if (!Inventory.Contains(w)) throw new InvalidOperationException($"В инвентаре нет предмета {w.Name}");
            EquippedWeapon = w;
            Console.WriteLine($"{Name} экипировал {w.Name}.");
        }

        // Надеть броню 
        public void EquipArmor(Armor a)
        {
            if (!Inventory.Contains(a)) throw new InvalidOperationException($"В инвентаре нет предмета {a.Name}");
            EquippedArmor = a;
            Console.WriteLine($"{Name} надел {a.Name}.");
            CalculateStats(); // пресчет КБ
        }

        // Добавить предмет в инвентарь
        public void AddToInventory(Item item)
        {
            Inventory.Add(item);
            Console.WriteLine($"В инвентарь добавлен предмет: {item}");
        }

        // Показать инвентарь
        public void ShowInventory()
        {
            Console.WriteLine($"--- Инвентарь {Name} ---");
            if (!Inventory.Any()) { Console.WriteLine("Пусто."); return; }
            for (int i = 0; i < Inventory.Count; i++) Console.WriteLine($"[{i}] {Inventory[i]}");
        }

        // Использование предмета
        public void UseItem(int index)
        {
            if (index < 0 || index >= Inventory.Count) { Console.WriteLine("Неверный индекс"); return; }
            var it = Inventory[index];
            if (it is Potion p) { p.Use(this); Inventory.RemoveAt(index); }
            else if (it is Artifact a) a.Use(this);
            else Console.WriteLine($"{it.Name} нельзя использовать прямо сейчас.");
        }

        // Восстановление маны 
        public void RestoreMana(int amount)
        {
            if (PlayerClass != PlayerClassEnum.Маг) { Console.WriteLine("Вы не маг, у вас нет маны."); return; }
            Mana += amount;
            if (Mana > MaxMana + PermanentManaBonus) Mana = MaxMana + PermanentManaBonus;
            Console.WriteLine($"{Name} восстановил {amount} MP. Мана: {Mana}/{MaxMana + PermanentManaBonus}");
        }

        // Быстрое лечение
        public void TakeHealing(int amount)
        {
            Health += amount;
            if (Health > MaxHealth) Health = MaxHealth;
            Console.WriteLine($"{Name} восстановил {amount} HP. HP: {Health}/{MaxHealth}");
        }

        // Логика атаки игрока по врагу
        public void PlayerAttack(Enemy enemy)
        {
            // бросок d20
            int roll = Utils.rnd.Next(1, 21);
            int attackBonus = EquippedWeapon != null ? (EquippedWeapon.DamageMax / 2) : 0;

            Console.WriteLine($"Пробитие КБ кубиком d20={roll}. {roll}+{attackBonus} vs {enemy.ArmorClass}");

            // Если 1 — автоматический промах и потеря хода
            if (roll == 1)
            {
                Console.WriteLine("Критическая неудача! Потеря хода.");
                return;
            }

            // Условие попадания: сумма броска и бонуса >= КБ цели или натуральные 20 
            bool hit = (roll + attackBonus >= enemy.ArmorClass) || roll == 20;
            if (!hit)
            {
                Console.WriteLine("Промах.");
                return;
            }

            // Расчёт урона: бросок урона в диапазоне оружия + бонусы
            int damageRoll = EquippedWeapon != null ? Utils.rnd.Next(EquippedWeapon.DamageMin, EquippedWeapon.DamageMax + 1) : 1;
            int affinityBonus = (EquippedWeapon != null && EquippedWeapon.Affinity == PlayerClass) ? ClassBonuses.WeaponAffinityBonus : 0;
            int damageBonus = affinityBonus + PermanentDamageBonus + (EquippedWeapon?.DamageMax / 4 ?? 0);
            int baseDamage = damageRoll + damageBonus;

            bool critical = roll == 20;
            int totalDamage = critical ? baseDamage * 2 + CritDamageBonus : baseDamage;

            if (!critical)
                Console.WriteLine($"Урон: бросок={damageRoll}. {damageRoll}+{damageBonus}={baseDamage} урона наносит {Name} по {enemy.Name}.");
            else
                Console.WriteLine($"Критическая удача! Урон: бросок={damageRoll}. ({damageRoll}+{damageBonus})*2 +{CritDamageBonus} = {totalDamage} урона наносит {Name} по {enemy.Name}.");

            enemy.TakeDamage(totalDamage);
        }

        // Магические способности
        public void CastFireball(Enemy target)
        {
            if (PlayerClass != PlayerClassEnum.Маг) { Console.WriteLine("Вы не маг."); return; }
            if (Mana < 8) throw new InvalidOperationException("Недостаточно маны.");
            Mana -= 8;
            int damage = 18 + PermanentDamageBonus;
            Console.WriteLine($"{Name} использует Огненный шар: наносит {damage} урона.");
            target.TakeDamage(damage);
        }

        public void CastHeal()
        {
            if (PlayerClass != PlayerClassEnum.Маг) { Console.WriteLine("Вы не маг."); return; }
            if (Mana < 6) throw new InvalidOperationException("Недостаточно маны.");
            Mana -= 6;
            int heal = 20;
            TakeHealing(heal);
            Console.WriteLine($"{Name} использовал Исцеление и восстановил {heal} HP.");
        }

        public void CastFreezingRay(Enemy target)
        {
            if (PlayerClass != PlayerClassEnum.Маг) { Console.WriteLine("Вы не маг."); return; }
            if (Mana < 5) throw new InvalidOperationException("Недостаточно маны.");
            Mana -= 5;
            int damage = 10 + PermanentDamageBonus;
            target.ArmorClass = Math.Max(1, target.ArmorClass - 2); // снижение КБ цели
            Console.WriteLine($"{Name} использует Ледяной луч: {damage} урона и -2 к КБ у цели.");
            target.TakeDamage(damage);
        }

        // Показать текущие характеристики игрока
        public void ShowStats()
        {
            Console.WriteLine($"--- {Name} ---");
            Console.WriteLine($"Класс: {PlayerClass}  Раса: {Race}");
            Console.WriteLine($"HP: {Health}/{MaxHealth}  КБ: {ArmorClass}  Оружие: {(EquippedWeapon?.ToString() ?? "Нет")}  Броня: {(EquippedArmor?.ToString() ?? "Нет")}");
            if (PlayerClass == PlayerClassEnum.Маг) Console.WriteLine($"Мана: {Mana}/{MaxMana + PermanentManaBonus}");
            Console.WriteLine($"Постоянный бонус к урону: {PermanentDamageBonus}. Постоянный бонус к КБ: {PermanentArmorBonus}");
        }
    }

    
    public class Enemy : Character
    {
        public EnemyRaceEnum Race { get; set; }
        public EnemyTypeSimple Type { get; set; }

        public Enemy(string name, EnemyRaceEnum race, EnemyTypeSimple type) : base(name)
        {
            Race = race; Type = type;
            CalculateStats(); 
        }

        public override void CalculateStats()
        {
            // Базовые значения
            MaxHealth = 30;
            ArmorClass = 8;

            // Для вражеских рас задаем уникальные параметры:
            if (Race == EnemyRaceEnum.РазумныйГриб)
            {
                MaxHealth = 28;
                ArmorClass = 7;
                EquippedWeapon = new Weapon("Когти гриба", 0, Rarity.Обычное, WeaponCategory.Кулак, 2, 5, null);
            }
            else if (Race == EnemyRaceEnum.Мимик)
            {
                MaxHealth = 40;
                ArmorClass = 10;
                EquippedWeapon = new Weapon("Укус мимика", 0, Rarity.Редкое, WeaponCategory.Кулак, 4, 8, null);
            }
            else if (Race == EnemyRaceEnum.Гоблин)
            {
                MaxHealth = 20;
                ArmorClass = 7;
                EquippedWeapon = new Weapon("Кинжал гоблина", 0, Rarity.Обычное, WeaponCategory.Кинжал, 3, 6, null);
            }
            else if (Race == EnemyRaceEnum.Огр)
            {
                MaxHealth = 60;
                ArmorClass = 11;
                EquippedWeapon = new Weapon("Тяжёлый булав", 0, Rarity.Редкое, WeaponCategory.Меч, 8, 14, null);
            }

            if (Race != EnemyRaceEnum.РазумныйГриб && Race != EnemyRaceEnum.Мимик)
            {
                switch (Type)
                {
                    case EnemyTypeSimple.Воин: MaxHealth += 8; ArmorClass += 1; break;
                    case EnemyTypeSimple.Маг:
                        MaxHealth -= 4; ArmorClass -= 1;
                        EquippedWeapon = new Weapon("Палочка мага", 1, Rarity.Обычное, WeaponCategory.Посох, 2, 5, null);
                        break;
                    case EnemyTypeSimple.Лучник:
                        EquippedWeapon = new Weapon("Лук лучника", 1, Rarity.Обычное, WeaponCategory.Лук, 3, 7, null);
                        break;
                    case EnemyTypeSimple.Берсерк:
                        MaxHealth += 15; ArmorClass -= 1;
                        EquippedWeapon = new Weapon("Топор берсерка", 1, Rarity.Редкое, WeaponCategory.Меч, 6, 12, null);
                        break;
                }
            }

            // Устанавливаем текущее здоровье в максимум
            Health = MaxHealth;
        }

       
        public void Attack(Player player)
        {
            if (!IsAlive) return;
            int roll = Utils.rnd.Next(1, 21);
            int attackBonus = EquippedWeapon != null ? (EquippedWeapon.DamageMax / 2) : 0;

            Console.WriteLine($"Пробитие брони кубиком d20={roll}. {roll}+{attackBonus} vs {player.ArmorClass}.");

            if (roll == 1)
            {
                Console.WriteLine($"{Name} — Критическая неудача!");
                return;
            }

            bool hit = (roll + attackBonus >= player.ArmorClass) || (roll == 20);
            if (!hit)
            {
                Console.WriteLine($"{Name} промахнулся.");
                return;
            }

            // Урон - бросок в диапазоне оружия
            int damageRoll = Utils.rnd.Next(EquippedWeapon.DamageMin, EquippedWeapon.DamageMax + 1);
            int damageBonus = 0;
            int baseDamage = damageRoll + damageBonus;
            bool critical = roll == 20;
            int totalDamage = critical ? baseDamage * 2 : baseDamage;

            if (!critical)
                Console.WriteLine($"Урон: бросок={damageRoll}. {damageRoll}+{damageBonus}={baseDamage} урона наносит {Name} по {player.Name}.");
            else
                Console.WriteLine($"Критический удача! Урон: бросок={damageRoll}. ({damageRoll}+{damageBonus})*2 = {totalDamage} урона наносит {Name} по {player.Name}.");

            player.TakeDamage(totalDamage);
        }

        // Генерация лута
        public Item GenerateLoot()
        {
            var pool = ItemDatabase.AllItems.Where(i => i.Rarity == Rarity.Обычное || i.Rarity == Rarity.Редкое).ToList();
            if (!pool.Any()) return null;
            return pool[Utils.rnd.Next(pool.Count)];
        }

        public override string ToString() => $"{Name} HP:{Health}/{MaxHealth} КБ:{ArmorClass} (оружие: {EquippedWeapon?.Name ?? "Нет"})";
    }

    // Списки оружия, брони, артефактов и зелей.
   
    static class ItemDatabase
    {
        public static List<Weapon> Weapons { get; private set; }
        public static List<Armor> Armors { get; private set; }
        public static List<Artifact> Artifacts { get; private set; }
        public static List<Potion> Potions { get; private set; }

        public static List<Item> AllItems
        {
            get
            {
                var all = new List<Item>();
                all.AddRange(Weapons);
                all.AddRange(Armors);
                all.AddRange(Artifacts);
                all.AddRange(Potions);
                return all;
            }
        }

        static ItemDatabase()
        {
            InitWeapons();
            InitArmors();
            InitArtifacts();
            InitPotions();
        }

        // Оружие
        static void InitWeapons()
        {
            Weapons = new List<Weapon>()
            {
                // обычные
                new Weapon("Ржавый клинок", 5, Rarity.Обычное, WeaponCategory.Кинжал, 3, 5, null),
                new Weapon("Короткий меч", 20, Rarity.Обычное, WeaponCategory.Меч, 6, 10, PlayerClassEnum.Воин),
                new Weapon("Легкий лук", 18, Rarity.Обычное, WeaponCategory.Лук, 5, 9, PlayerClassEnum.Лучник),

                // редкие
                new Weapon("Боевой меч", 60, Rarity.Редкое, WeaponCategory.Меч, 10, 15, PlayerClassEnum.Воин),
                new Weapon("Охотничий лук", 55, Rarity.Редкое, WeaponCategory.Лук, 9, 14, PlayerClassEnum.Лучник),
                new Weapon("Посох ученика", 40, Rarity.Редкое, WeaponCategory.Посох, 6, 12, PlayerClassEnum.Маг),

                // эпические
                new Weapon("Посох адепта", 140, Rarity.Эпическое, WeaponCategory.Посох, 8, 15, PlayerClassEnum.Маг),
                new Weapon("Боевой лук элита", 160, Rarity.Эпическое, WeaponCategory.Лук, 12, 18, PlayerClassEnum.Лучник),
                new Weapon("Меч рыцаря", 180, Rarity.Эпическое, WeaponCategory.Меч, 14, 20, PlayerClassEnum.Воин)
            };
        }

        // Броня
        static void InitArmors()
        {
            Armors = new List<Armor>()
            {
                // обычные
                new Armor("Кожаный доспех", 20, Rarity.Обычное, ArmorCategory.Лёгкая, 5, null),
                new Armor("Лёгкая броня", 25, Rarity.Обычное, ArmorCategory.Лёгкая, 6, PlayerClassEnum.Лучник),
                new Armor("Мантия ученика", 15, Rarity.Обычное, ArmorCategory.Мантия, 3, PlayerClassEnum.Маг),

                // редкие
                new Armor("Кольчужка", 60, Rarity.Редкое, ArmorCategory.Средняя, 8, null),
                new Armor("Средняя броня", 80, Rarity.Редкое, ArmorCategory.Средняя, 9, PlayerClassEnum.Воин),
                new Armor("Мантия мага", 90, Rarity.Редкое, ArmorCategory.Мантия, 4, PlayerClassEnum.Маг),

                // эпические
                new Armor("Латы рыцаря", 200, Rarity.Эпическое, ArmorCategory.Тяжёлая, 12, PlayerClassEnum.Воин),
                new Armor("Доспех следопыта", 180, Rarity.Эпическое, ArmorCategory.Средняя, 10, PlayerClassEnum.Лучник),
                new Armor("Мантия архимага", 220, Rarity.Эпическое, ArmorCategory.Мантия, 5, PlayerClassEnum.Маг)
            };
        }

        // Каждый артефакт имеет свой эффект
        static void InitArtifacts()
        {
            Artifacts = new List<Artifact>()
            {
                // обычные
                new Artifact("Кольцо силы", 50, Rarity.Обычное, "+2 к урону", p => { p.PermanentDamageBonus += 2; }),
                new Artifact("Амулет мудрости", 60, Rarity.Обычное, "+5 к мане", p => { p.PermanentManaBonus += 5; p.MaxMana += 5; p.Mana += 5; }),
                new Artifact("Камень жизни", 70, Rarity.Обычное, "+10 к HP (постоянно)", p => { p.MaxHealth += 10; p.Health += 10; }),

                // редкие
                new Artifact("Сапоги стремительности", 120, Rarity.Редкое, "+3 к уклонению, первый удар удваивает урон", p => { p.EvasionBonus += 3; }),
                new Artifact("Око тьмы", 140, Rarity.Редкое, "+2 к криту; при крите снижает КБ цели", p => { p.CritDamageBonus += 2; }),
                new Artifact("Перстень защиты", 130, Rarity.Редкое, "+3 к КБ и +1 к уклонению", p => { p.PermanentArmorBonus += 3; p.EvasionBonus += 1; }),

                // эпические
                new Artifact("Амулет силы разума", 350, Rarity.Эпическое, "+10 к мане, +3 к критам заклинаний", p => { p.PermanentManaBonus += 10; p.MaxMana += 10; p.Mana += 10; }),
                new Artifact("Меч судьбы", 380, Rarity.Эпическое, "+5 к урону, +2 к криту", p => { p.PermanentDamageBonus += 5; p.CritDamageBonus += 2; }),
                new Artifact("Лук ветров", 400, Rarity.Эпическое, "+4 к дальнему урону, +1 к криту", p => { p.PermanentDamageBonus += 4; p.CritDamageBonus += 1; })
            };
        }

        // Зелья лечения и маны
        static void InitPotions()
        {
            Potions = new List<Potion>()
            {
                new Potion("Зелье лечения", 10, Rarity.Обычное, healAmount: 20),
                new Potion("Зелье маны", 12, Rarity.Обычное, manaAmount: 15)
            };
        }

        // Получить случайный предмет заданной редкости
        public static Item GetRandomItemWithRarities(params Rarity[] rarities)
        {
            var pool = AllItems.Where(i => rarities.Contains(i.Rarity)).ToList();
            if (!pool.Any()) return null;
            return pool[Utils.rnd.Next(pool.Count)];
        }

        // Получить случайный предмет конкретной категории с указанной редкостью
        public static Item GetRandomItemFromCategoryAndRarity(Type categoryType, Rarity rarity)
        {
            if (categoryType == typeof(Weapon))
                return Weapons.Where(w => w.Rarity == rarity).OrderBy(x => Utils.rnd.Next()).FirstOrDefault();
            if (categoryType == typeof(Armor))
                return Armors.Where(w => w.Rarity == rarity).OrderBy(x => Utils.rnd.Next()).FirstOrDefault();
            if (categoryType == typeof(Artifact))
                return Artifacts.Where(w => w.Rarity == rarity).OrderBy(x => Utils.rnd.Next()).FirstOrDefault();
            if (categoryType == typeof(Potion))
                return Potions.Where(w => w.Rarity == rarity).OrderBy(x => Utils.rnd.Next()).FirstOrDefault();

            return null;
        }
    }

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
