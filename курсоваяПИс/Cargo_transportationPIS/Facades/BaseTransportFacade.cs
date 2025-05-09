using Azure.Core;
using Cargo_transportationPIS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cargo_transportationPIS.Facades
{
    public class BaseTransportFacade : IManagerFacade, IClientFacade, IDriverFacade
    {
        private readonly GruzzPISEntities _context;

        public BaseTransportFacade()
        {
            _context = GruzzPISEntities.GetContext();
        }

        // Клиент
        //создание заявки
        public void CreateRequest(int clientId, DateTime departureDate,
                              string departureAddress, string arrivalAddress,
                              string cargoName, int weight, int volume,
                              string comment = "")
        {
            var client = _context.Пользователи.FirstOrDefault(u => u.ID_пользователя == clientId &&
                                                                u.ID_роли == 3); // 3 = Клиент
            if (client == null)
                throw new InvalidOperationException("Клиент не найден или не имеет соответствующих прав");

            var newRequest = new Заявки
            {
                ID_клиента = clientId,
                Дата = departureDate,
                Адрес_отправления = departureAddress,
                Адрес_прибытия = arrivalAddress,
                Название_груза = cargoName,
                Вес = weight,
                Количество = volume,
                Комментарий = comment,
                ID_статус = 1 // 1 = Отправлена
            };

            _context.Заявки.Add(newRequest);
            _context.SaveChanges();
        }

        //отмена заявки
        public void CancelRequest(int requestId)
        {
            var request = _context.Заявки.Find(requestId);
            if (request == null)
                throw new ArgumentException("Заявка не найдена");

            if (request.ID_статус == 2 || request.ID_статус == 3 || request.ID_статус == 7 || request.ID_статус == 4)
                throw new InvalidOperationException("Невозможно отменить заявку в процессе выполнения");

            request.ID_статус = 5; // 5 = Отменена
            _context.SaveChanges();
        }

        //редактирование заявки
        public void EditRequest(int requestId, string newDescription)
        {
            var request = _context.Заявки.Find(requestId);
            if (request == null)
                throw new ArgumentException("Заявка не найдена");

            request.Комментарий = newDescription;
            _context.SaveChanges();
        }

        
        // водитель
        //одобрить заявку на заказ
        public void ApproveRequest(int driverId, int requestId)
        {
            var driver = _context.Пользователи.Find(driverId);
            var request = _context.Заявки.Find(requestId);

            if (driver == null || driver.ID_роли != 4) // 4 = Водитель
                throw new InvalidOperationException("Водитель не найден или не имеет прав");

            if (request == null)
                throw new ArgumentException("Заявка не найдена");

            var transport = _context.Транспорт
                .FirstOrDefault(t => t.ID_статус == 11 && // 11 = Свободен
                                   Convert.ToInt32(t.Вместительность) >= request.Вес);

            if (transport == null)
                throw new InvalidOperationException("Нет доступного транспорта");

            var route = new Маршруты
            {
                ID_заявки = requestId,
                ID_водителя = driverId,
                ID_транспорта = transport.ID_транспорта,
                Дата_отправки = DateTime.Now,
                ID_статус = 2, // 2 выполняется 
                Стоимость = CalculateTransportCost(transport.ID_транспорта, Convert.ToInt32( request.Вес))
            };

            request.ID_статус = 2; // выполняется
            transport.ID_статус = 10; // Занят

            _context.Маршруты.Add(route);
            _context.SaveChanges();
        }

        //запрос на премию
        public void RequestBonusPayment(int driverId)
        {
            var driver = _context.Пользователи.Find(driverId);
            if (driver == null || driver.ID_роли != 4) // 4 = Водитель
                throw new InvalidOperationException("Водитель не найден");

            decimal bonus = CalculateDriverBonus(driverId);

            var payment = new Платежи
            {
                Сумма = bonus,
                Дата_платежа = DateTime.Now,
                ID_статуса = 13
            };

            _context.Платежи.Add(payment);
            _context.SaveChanges();
        }

        private decimal CalculateDriverBonus(int driverId)
        {
            var driver = _context.Пользователи.Find(driverId);
            if (driver == null)
                throw new ArgumentException("Водитель не найден");

            var currentMonthRoutes = _context.Маршруты
                .Where(r => r.ID_водителя == driverId &&
                           r.Дата_отправки == DateTime.Now)
                .ToList();

            decimal totalEarnings = currentMonthRoutes.Sum(r => r.Стоимость ?? 0);
            decimal bonusPercent = Math.Min(Convert.ToInt32(driver.Стаж) * 0.05m, 0.25m);

            if (currentMonthRoutes.Count > 5)
                bonusPercent += 0.1m;

            return totalEarnings * bonusPercent;
        }

        public void ConfirmArrival(int routeId)
        {
            var route = _context.Маршруты.Find(routeId);
            if (route == null)
                throw new ArgumentException("Маршрут не найден");

            route.Дата_прибытия = DateTime.Now;
            route.ID_статус = 5; // 5 = Завершен

            var transport = _context.Транспорт.Find(route.ID_транспорта);
            transport.ID_статус = 11; // 1 = Доступен

            _context.SaveChanges();
        }

        // менеджер
        //добавить водителя
        public void AddDriver(string lastName, string firstName, string middleName, int experience, string passport)
        {
            var newDriver = new Пользователи
            {
                Фамилия = lastName,
                Имя = firstName,
                Отчество = middleName,
                Стаж = experience,
                Паспорт = passport,
                ID_роли = 4, // 4 = Водитель
                Дата_рождения = DateTime.Now.AddYears(-25)
            };

            _context.Пользователи.Add(newDriver);
            _context.SaveChanges();
        }

        //назначение зарплаты
        public void UpdateDriverBonus(int driverId, decimal bonus)
        {
            var driver = _context.Пользователи.Find(driverId);
            if (driver == null || driver.ID_роли != 4) // 4 = Водитель
                throw new InvalidOperationException("Водитель не найден");

            driver.Зарплата = bonus;
            _context.SaveChanges();
        }

        //создание отчета по водителям
        public void CreateReport(DateTime startDate, DateTime endDate)
        {
            var reportData = _context.Маршруты
                .Where(r => r.Дата_отправки >= startDate && r.Дата_отправки <= endDate)
                .GroupBy(r => r.ID_водителя)
                .Select(g => new
                {
                    DriverId = g.Key,
                    TotalRoutes = g.Count(),
                    TotalIncome = g.Sum(r => r.Стоимость)
                })
                .ToList();            
        }

        
        public void SendOrderRequestToDriver(int driverId, int requestId)
        {
            var driver = _context.Пользователи.Find(driverId);
            var request = _context.Заявки.Find(requestId);

            if (driver == null || driver.ID_роли != 4) // 4 = Водитель
                throw new InvalidOperationException("Водитель не найден");

            if (request == null)
                throw new ArgumentException("Заявка не найдена");

            var notification = new Уведомления
            {
                ID_пользователя = driverId,
                ID_заявки = requestId,
                Текст = $"Новая заявка на перевозку: {request.Название_груза}",
                Дата_создания = DateTime.Now,
                Прочитано = false
            };

            _context.Уведомления.Add(notification);
            _context.SaveChanges();
        }
        //создание перевозки
        public void CreateTransport(int requestId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var request = _context.Заявки.FirstOrDefault(z => z.ID_заявки == requestId);

                    if (request == null)
                        throw new ArgumentException($"Заявка с ID {requestId} не найдена.");

                   
                    // Поиск доступного транспорта с подходящей грузоподъемностью
                    var availableTransport = _context.Транспорт
                        .FirstOrDefault(t =>
                            t.ID_статус == 11 &&
                           t.Вместительность >= request.Вес);

                    if (availableTransport == null)
                        throw new InvalidOperationException("Нет доступного транспорта подходящей грузоподъемности.");

                    // Создание маршрута
                    var route = new Маршруты
                    {
                        ID_заявки = requestId,
                        ID_транспорта = availableTransport.ID_транспорта,
                        Дата_отправки = DateTime.Now,
                        ID_статус = 7, // Ожидает водителя
                        Стоимость = CalculateTransportCost(availableTransport.ID_транспорта, request.Вес.Value)
                };

                    // Обновление статусов
                    request.ID_статус = 2;
                    availableTransport.ID_статус = 10;

                    _context.SaveChanges();

                    // Пометить измененные сущности
                   _context.Маршруты.Add(route);

                    // Сохранение изменений
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    // Логирование детальной информации об ошибке
                    var errorMessage = $"Ошибка при обновлении базы данных: {dbEx.InnerException?.Message ?? dbEx.Message}";
                    Console.WriteLine(errorMessage);
                    throw; // Перебрасываем исключение дальше
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    var errorMessage = $"Произошла ошибка: {ex.Message}";
                    Console.WriteLine(errorMessage);
                    throw; // Перебрасываем исключение дальше
                }
            }
        }


        //public void CreateTransport(string licensePlate, string type, string capacity, int year)
        //{
        //    var newTransport = new Транспорт
        //    {
        //        Номерной_знак = licensePlate,
        //        Тип = type,
        //        Вместительность = capacity,
        //        Год_выпуска = year,
        //        ID_статус = 1 // 1 = Доступен
        //    };

        //    _context.Транспорт.Add(newTransport);
        //    _context.SaveChanges();
        //}

        //назначение стоимости перевозке
        public decimal CalculateTransportCost(int transportId, int cargoWeight)
        {
            // Находим транспорт по ID
            var transport = _context.Транспорт.Find(transportId);
            if (transport == null)
                throw new ArgumentException("Транспорт не найден");

            decimal baseCost = 1000m;
            decimal weightCost = cargoWeight * 10m;
            decimal distanceCost = 500 * 50m;

            decimal transportCoefficient;

            // Используем if-else для определения коэффициента
            if (transport.Тип == "Рефрижератор")
            {
                transportCoefficient = 1.5m;
            }
            else if (transport.Тип == "Крупногабаритный")
            {
                transportCoefficient = 1.3m;
            }
            else
            {
                transportCoefficient = 1.0m;
            }

            return (baseCost + weightCost + distanceCost) * transportCoefficient;
        }


        //архивирование водителя
        public void BlockDriver(int driverId)
        {
            var driver = _context.Пользователи.Find(driverId);
            if (driver == null || driver.ID_роли != 4) // 4 = Водитель
                throw new InvalidOperationException("Водитель не найден");

            driver.Активен = false;
            _context.SaveChanges();
        }

        //архивирование транспорта
        public void EditAndArchiveTransport(int transportId)
        {
            var transport = _context.Транспорт.Find(transportId);
            if (transport == null)
                throw new ArgumentException("Транспорт не найден");

            transport.ID_статус = 12;
            _context.SaveChanges();
        }
    }
}