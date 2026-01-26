// --- ЗАДАНИЕ 1: Модуль Инвентаря (Closures) ---
// В C# мы бы сделали private List<string> _items;
// В JS мы используем замыкание.

function createInventory(maxCapacity) {
    // Эта переменная "замкнута" (Captured). Она приватная.
    let items = [];

    return {
        add: function(item) {
            if (items.length >= maxCapacity) {
                console.log(`Inventory full! Cannot add ${item}`);
                return false;
            }
            items.push(item);
            console.log(`Added ${item}. Count: ${items.length}`);
            return true;
        },
        // Задача: Добавить метод getItems(), который возвращает КОПИЮ массива
        // (чтобы снаружи нельзя было мутировать приватный items через ссылку)
        getItems: function() {
            let copyItems = items;
            return [copyItems]; 
        }
    };
}

const backpack = createInventory(2);
backpack.add("Potion");
backpack.add("Sword");
backpack.add("Shield"); // Inventory full
// console.log(backpack.items); // undefined (Приватность работает!)


// --- ЗАДАНИЕ 2: The 'this' Trap ---

const hero = {
    name: "Arthas",
    health: 100,
    heal: function(amount) {
        this.health += amount;
        console.log(`${this.name} healed by ${amount}. HP: ${this.health}`);
    }
};

// Сценарий: Мы передаем метод как колбэк в систему событий (или таймер)
function scheduleAction(actionFn) {
    console.log("System: Executing action in 100ms...");
    // Эмуляция потери контекста (вызов функции без точки)
    setTimeout(actionFn, 100); 
}

console.log("--- Context Test ---");
// ОШИБКА:
scheduleAction(()=>hero.heal(10)); // Выведет "undefined healed..." или NaN

// ЗАДАЧА: Исправь вызов выше ТРЕМЯ способами:
// 1. Через замыкание (Wrapper)
// scheduleAction(() => ... );

// 2. Через .bind()
// scheduleAction(...);

// 3. (Продвинутый) Измени сам объект hero, чтобы heal была стрелочной функцией (если бы это был класс)
// Но так как это объект-литерал, тут лучше показать call/apply решение:
// scheduleAction(function() { hero.heal.call(hero, 20); });


// --- ЗАДАНИЕ 3: LINQ to JS ---

const loot = [
    { name: "Rusty Sword", value: 5, type: "Weapon" },
    { name: "Golden Coin", value: 1, type: "Currency" },
    { name: "Magic Staff", value: 150, type: "Weapon" },
    { name: "Apple", value: 2, type: "Food" },
    { name: "Diamond", value: 500, type: "Currency" }
];

// C# Query:
// var result = loot.Where(x => x.Value > 10).Select(x => x.Name).Aggregate((a, b) => a + ", " + b);

console.log("--- LINQ Test ---");

// ЗАДАЧА: Реализуй тот же пайплайн на JS
const result = loot
    // 1. Filter: Оставить предметы дороже 10 золотых
    .filter(item => item.value > 10)
    // 2. Map: Превратить объекты в строки имен
    .map(item => item.name)
    // 3. Reduce: Склеить в одну строку через запятую
    .reduce((acc, current) => acc + ", " + current);

console.log("Rich Loot:", result);