using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp1.model;

namespace WpfApp1.Repositories
{
    public interface IAssignmentRepository
    {
        Task<IEnumerable<AssignedItemModel>> GetAllAsync();
        Task<IEnumerable<AssignedItemModel>> GetPendingAssignmentsAsync();
        Task<AssignedItemModel> AssignAsync(int employeeId, int productId);
        Task<bool> UnassignAsync(int assignmentId);
        Task<AssignedItemModel> GetLatestByProductIdAsync(int productId);
    }
}
