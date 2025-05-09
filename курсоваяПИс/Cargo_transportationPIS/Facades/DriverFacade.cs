using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cargo_transportationPIS.Facades
{
    public interface IDriverFacade
    {
        void ApproveRequest(int driverId, int requestId);//одобрить заявку на заказ
        void RequestBonusPayment(int driverId); //запрос на премии
        void ConfirmArrival(int requestId); //подтвердить прибытие
    }
}
