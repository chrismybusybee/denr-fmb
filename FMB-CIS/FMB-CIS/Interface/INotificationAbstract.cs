using FMB_CIS.Models;

namespace FMB_CIS.Interface
{
    public interface INotificationAbstract : IAbstractEntry<tbl_notifications>
    {
        void Insert(tbl_notifications model, int userId);
    }
}
