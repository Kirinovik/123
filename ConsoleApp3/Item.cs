using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
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
}
//предметы