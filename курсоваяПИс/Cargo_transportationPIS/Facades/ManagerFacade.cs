using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo_transportationPIS.Facades
{
    public interface IManagerFacade
    {
        void AddDriver(string lastName, string firstName, string middleName, int experience, string passport); //добавить водителя
        void UpdateDriverBonus(int driverId, decimal bonus);
        void CreateReport(DateTime startDate, DateTime endDate);
      //  void CalculateDriverPerformanceStatistics();
        void SendOrderRequestToDriver(int driverId, int requestId);
       void CreateTransport(int requestId);//создание маршрута
        void BlockDriver(int driverId);
        void EditAndArchiveTransport(int transportId);
    }
}
