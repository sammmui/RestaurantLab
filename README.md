# RestaurantLab

Лабораторна робота з дисципліни «Проектування баз даних та інформаційних систем».
Тема: *система управління ресторанним бізнесом*.

Проєкт — це REST API на ASP.NET Core над базою даних MS SQL Server. Доступ до даних реалізовано патернами **Repository + Unit of Work**: читання виконується тільки через розрізи даних (Views), а запис — тільки через збережені процедури.

**Виконала:** Балєєва Марія, група ТТП-31, Київський національний університет імені Тараса Шевченка.

## Технології

- MS SQL Server (LocalDB)
- C#, .NET 10, ASP.NET Core Web API
- Entity Framework Core (використовується лише для підключення та виконання SQL-команд)
- Swagger UI (Swashbuckle) для тестування ендпоінтів

## База даних

- 27 таблиць зі зв'язками (Foreign Keys), схема: <https://dbdocs.io/samui/lab1>
- *Soft delete* (поле `is_deleted`) і аудит змін (`last_modified_date`, `modified_by_user_id`) у ключових сутностях
- 13 програмованих об'єктів:
  - 5 збережених процедур: `usp_CreateOrder`, `usp_AddOrderItem`, `usp_SoftDeleteMenuItem`, `usp_SoftDeleteEmployee`, `usp_RestoreEmployee`
  - 2 функції: `fn_GetEmployeeShiftHours`, `fn_IsTableAvailable`
  - 3 тригери: оновлення `last_modified_date` для `roles`, `employees`, `orders`
  - 3 розрізи даних: `vw_ActiveMenuItems`, `vw_ActiveEmployees`, `vw_OrderLines`
- Індекси: унікальні, фільтровані (Filtered) та покриваючий (Covering)

## Структура проєкту

```
RestaurantLab
├── Controllers      # HTTP-ендпоінти (Menu, Employees, Orders)
├── Data             # RestaurantDbContext
├── Models           # DTO для View та моделі запитів
├── Repositories     # Інтерфейси та реалізації репозиторіїв
├── UoW              # Unit of Work (керування транзакцією)
├── Database         # SQL-скрипти
├── appsettings.json # Рядок підключення
└── Program.cs
```

## Як запустити

1. Створіть базу даних `RestaurantDB` у SQL Server або LocalDB.
2. Виконайте скрипти з папки `Database/` у такому порядку:
   1. `lab1.sql` — таблиці та зв'язки
   2. `procedures.sql` — тригери, процедури, функції
   3. `views.sql` — розрізи даних
   4. `indexes.sql` — індекси
   5. `seed.sql` — початкові тестові дані
3. Перевірте рядок підключення в `appsettings.json`:

   ```json
   "RestaurantDb": "Server=(localdb)\\MSSQLLocalDB;Database=RestaurantDB;Trusted_Connection=True;TrustServerCertificate=True;"
   ```

   Якщо у вас інший сервер, змініть значення після `Server=`.
4. Запустіть проєкт у Visual Studio (F5) або командою `dotnet run`.
5. Відкрийте Swagger: `http://localhost:5000/swagger` (порт дивіться в консолі після запуску).

## Ендпоінти

| Метод | Ендпоінт | Об'єкт БД | Відповідь |
|---|---|---|---|
| GET | `/api/Menu` | View `vw_ActiveMenuItems` | 200 |
| DELETE | `/api/Menu/{id}?userId=` | `usp_SoftDeleteMenuItem` | 204 / 404 |
| GET | `/api/Employees` | View `vw_ActiveEmployees` | 200 |
| DELETE | `/api/Employees/{id}?userId=` | `usp_SoftDeleteEmployee` | 204 / 404 |
| POST | `/api/Employees/{id}/restore?userId=` | `usp_RestoreEmployee` | 204 / 404 |
| POST | `/api/Orders` | `usp_CreateOrder` + `usp_AddOrderItem` (одна транзакція) | 201 / 400 |
| GET | `/api/Orders/{id}` | View `vw_OrderLines` | 200 / 404 |

### Приклад створення замовлення

`POST /api/Orders`

```json
{
  "employeeId": 1,
  "tableId": 1,
  "customerId": null,
  "userId": 1,
  "items": [
    { "menuItemId": 1, "quantity": 2 },
    { "menuItemId": 2, "quantity": 1 }
  ]
}
```

Якщо будь-яка позиція недоступна (наприклад, неіснуюча страва), процедура повертає помилку, `UnitOfWork` відкочує всю транзакцію разом із щойно створеним замовленням, а клієнт отримує відповідь `400`.

## Патерни

- **Repository**: `OrderRepository`, `MenuItemRepository`, `EmployeeRepository`. Кожен читає дані з відповідного View і змінює їх через процедури.
- **Unit of Work**: клас `UnitOfWork` створює всі репозиторії з одним спільним `DbContext`, тому всі операції виконуються в межах одного з'єднання та однієї транзакції (`BeginAsync`, `CommitAsync`, `RollbackAsync`). Для кожного HTTP-запиту створюється окремий екземпляр (`AddScoped`).
