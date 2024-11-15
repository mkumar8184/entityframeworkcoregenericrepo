# Generic repository  In entity framework core and sql server

# why you should use generic repository pattern
1. Code Reusability
2. Easy maintainablity
3. consistenly
4. Seperation of concerns -no dependency between data and business logic
5. Reduce writing complex query
6. Use multiple database
   
**What Extra in this code base**
1. Added session user so we can have always userid to use for created by or updated by
2. Implement transcations
3. Can pass multiple data base
4. All types of query .procedures ,inline query implementation
5. Filtering .ordering almost all required feature are availbe
   
   
# How to Use
1. Download code base
2. change below function as per your need to get user id and other data
   this function makes sure you have user id before inserting data or getting data from any table . you can change this as per need
   
 private void SetCompanyIdForEntity(T entity,bool isCreating=false)
 {
 
     // Get the CompanyId from HttpContext or where do you store
   
     var companyId = _httpContextAccessor.HttpContext?.Items["CompanyId"]?.ToString();
     var userId = _httpContextAccessor.HttpContext?.Items["UserId"]?.ToString();
     if (string.IsNullOrEmpty(companyId))
     {
         throw new Exception("CompanyId is not available in the current context.");
     }
     if(string.IsNullOrEmpty(userId))
     {
         throw new Exception("No user found to take actions in current context.");
     }

     // Convert to integer and set the CompanyId for the entity
     entity.CompanyId = Convert.ToInt32(companyId);
     entity.UpdatedBy = Convert.ToInt32(userId);
     if(isCreating)
     {
         entity.CreatedBy = Convert.ToInt32(userId);
     } 
}
     
  4. BaseEntity Class. you can defined common field here from data so you dont need to repeat that
     
public class BaseEntity
 {
     public int CreatedBy { get; set; }
     public DateTime CreatedOn { get; set; }=DateTime.Now;
     public DateTime UpdatedOn { get; set; } = DateTime.Now;
     public int UpdatedBy { get; set; }  
     public int CompanyId { get; set; }  // CompanyId field

 }
 
 how to call this , 
 lets say you have to save data from employee table so either you can directly use BaseRepository or for better segregation you can create another employee Repository to communite database like below


public interface IEmployeeDataRepository 
{
    Task<Employees> AddEmployee(EmployeeDataCommand command);
}

public class EmployeeDataRepository : BaseRepository<Employees, HRServiceHubContext>, IEmployeeDataRepository
{
    public EmployeeDataRepository(HRServiceHubContext context, IHttpContextAccessor httpContextAccessor)
   : base(context, httpContextAccessor)
    {
    // you can remove whatever not required in your case or handle as per your need .but you need to make chhange in baseRepository as well
        // The constructor passes the context and HttpContextAccessor to the base class (BaseRepository)
    }
}

    public async Task<Employees> AddEmployee(EmployeeDataCommand command)
    {
        try
        {
        // you can you automapper to map objects
            var mappedData = MappingProfile<EmployeeDataCommand, Employees>.Map(command);
            var data = await AddAsync(mappedData);
            return data;
        } catch (DbException ex)
        {
            throw ex;
        }
    
    }
}
What are the functions available
1. add
2. Update
3. Delete
4. AddBulk
5. UpdateBulk
6. DeleteBulk
7. GetById
8. GetByCustomFilter
9. GetPaginatedData with total record count
10. Custom filter

How to apply custom filter
public IEnumerable<Employees> GetEmployeeByUserId(string userId)
 {
     var filterExpression = Filter<UserProfileView>.Create(p => p.UserId== userId);           
     var result = Get(Query.WithFilter(filterExpression));// pass your filter expression
     return result;        
 }
antother linq query example 

   var result = Query.WithFilter(Filter<Employees>.Create(p =>
   p.EmployeeNmae == filterParam 
   && p.EmployeeId >= filterParam
   && p.PositionTitle == filterParam
  /// and so on
   ));
  return Get(result);

Benefits using generic repository pattern

Connect if need some more extra info 
Thanks 
  
