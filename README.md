# Generic Repository in Entity Framework Core and SQL Server

## Why Use Generic Repository Pattern?
1. **Code Reusability**  
   Reuse common CRUD operations for multiple entities.

2. **Easy Maintainability**  
   Centralized logic ensures easier updates and management.

3. **Consistency**  
   Provides a standardized way to handle data operations.

4. **Separation of Concerns**  
   No direct dependency between data access and business logic.

5. **Simplified Query Management**  
   Reduces the need for writing complex queries.

6. **Support for Multiple Databases**  
   Enables operations on multiple databases with minimal effort.

---

## What's Extra in This Codebase?
1. **Session User Integration**  
   Automatically manages `CreatedBy` and `UpdatedBy` fields based on the current user session.
1. **Stop Unauthorized user Transaction**  
   Automatically manages `CreatedBy` and `UpdatedBy` fields based on the current user session.
2. **Transaction Support**  
   Ensures transactional consistency across operations.

3. **Multiple Database Handling**  
   Flexibility to switch between databases.

4. **Support for All Query Types**  
   Includes procedures, inline queries, and advanced filtering.

5. **Advanced Features**  
   Built-in support for filtering, ordering, and other commonly required functionalities.

---

## How to Use
1. **Download Codebase**  
   Clone or download the repository.

2. **Modify the Following Function**  
   Update the `SetCompanyIdForEntity` function to align with your application's context:

```csharp
private void SetCompanyIdForEntity(T entity, bool isCreating = false)
{
    // Retrieve CompanyId and UserId from HttpContext
    var companyId = _httpContextAccessor.HttpContext?.Items["CompanyId"]?.ToString();
    var userId = _httpContextAccessor.HttpContext?.Items["UserId"]?.ToString();

    if (string.IsNullOrEmpty(companyId))
    {
        throw new Exception("CompanyId is not available in the current context.");
    }
    if (string.IsNullOrEmpty(userId))
    {
        throw new Exception("No user found to take actions in current context.");
    }

    // Set values for the entity
    entity.CompanyId = Convert.ToInt32(companyId);
    entity.UpdatedBy = Convert.ToInt32(userId);
    if (isCreating)
    {
        entity.CreatedBy = Convert.ToInt32(userId);
    }
}

```csharp
public class BaseEntity
{
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime UpdatedOn { get; set; } = DateTime.Now;
    public int UpdatedBy { get; set; }
    public int CompanyId { get; set; } // CompanyId field
}
4. **Creating Specific Repositories**
For better segregation, extend the BaseRepository for specific entities like Employee.
```csharp
public interface IEmployeeDataRepository
{
    Task<Employees> AddEmployee(EmployeeDataObject obj);
}

public class EmployeeDataRepository : BaseRepository<Employees, HRServiceHubContext>, IEmployeeDataRepository
{
    public EmployeeDataRepository(HRServiceHubContext context, IHttpContextAccessor httpContextAccessor)
        : base(context, httpContextAccessor)
    {
    }

    public async Task<Employees> AddEmployee(EmployeeDataObject obj)
    {
        try
        {
            // Map command to entity using AutoMapper
            var mappedData = MappingProfile<EmployeeDataObject, Employees>.Map(obj);
            var data = await AddAsync(mappedData);
            return data;
        }
        catch (DbException ex)
        {
            throw ex;
        }
    }
}


## Available Functions
The repository includes the following operations:

Add
Update
Delete
AddBulk
UpdateBulk
DeleteBulk
GetById
GetByCustomFilter
GetPaginatedData (with total record count)
Custom Filter

##Example for custom filter
Example 1: Filter by User ID

public IEnumerable<Employees> GetEmployeeByUserId(string userId)
{
    var filterExpression = Filter<UserProfileView>.Create(p => p.UserId == userId);           
    var result = Get(Query.WithFilter(filterExpression)); // Apply filter expression
    return result;        
}
Example -2
var result = Query.WithFilter(Filter<Employees>.Create(p =>
    p.EmployeeName == filterParam 
    && p.EmployeeId >= filterParam
    && p.PositionTitle == filterParam
));
return Get(result);

Similary you can go through code base and explore all the features 

Benefits of the Generic Repository Pattern
Code Reusability
Ease of Maintenance
Consistency
Separation of Concerns
Support for Advanced Querying
Multi-Database Flexibility

**Note :some of the code base I had downloaded  from other's git repo long back and modified .
Thanks !! connect if you need more info
