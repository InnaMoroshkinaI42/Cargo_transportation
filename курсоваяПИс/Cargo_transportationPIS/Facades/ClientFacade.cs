using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo_transportationPIS.Facades
{
    public interface IClientFacade
    {
         void CreateRequest(int clientId, DateTime departureDate,
                              string departureAddress, string arrivalAddress,
                              string cargoName, int weight, int volume,
                              string comment = ""); //создание заявки
        void CancelRequest(int requestId); //отмена заявки
        void EditRequest(int requestId, string newDescription); //редактирование 
        //void TrackTransport(int transportId);//отслеживание транспорта 
    }
}
