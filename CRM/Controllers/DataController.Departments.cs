using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteDepartment/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteDepartment(Guid id)
    {
        var output = await da.DeleteDepartment(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteDepartmentGroup/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteDepartmentGroup(Guid id)
    {
        var output = await da.DeleteDepartmentGroup(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetDepartment/{id}")]
    public async Task<ActionResult<DataObjects.Department>> GetDepartment(Guid id)
    {
        var output = await da.GetDepartment(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/GetDepartmentGroup/{id}")]
    public async Task<ActionResult<DataObjects.DepartmentGroup>> GetDepartmentGroup(Guid id)
    {
        var output = await da.GetDepartmentGroup(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetDepartmentGroups")]
    public async Task<ActionResult<List<DataObjects.DepartmentGroup>>> GetDepartmentGroups()
    {
        var output = await da.GetDepartmentGroups(CurrentUser.TenantId, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetDepartments")]
    public async Task<ActionResult<List<DataObjects.Department>>> GetDepartments()
    {
        var output = await da.GetDepartments(CurrentUser.TenantId, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SaveDepartment")]
    public async Task<ActionResult<DataObjects.Department>> SaveDepartment(DataObjects.Department department)
    {
        var output = await da.SaveDepartment(department, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SaveDepartmentGroup/")]
    public async Task<ActionResult<DataObjects.DepartmentGroup>> SaveDepartmentGroup(DataObjects.DepartmentGroup departmentGroup)
    {
        var output = await da.SaveDepartmentGroup(departmentGroup, CurrentUser);
        return Ok(output);
    }
}
