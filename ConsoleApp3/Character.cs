using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
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
}
