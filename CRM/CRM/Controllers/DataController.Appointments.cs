using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

public partial class DataController
{
    [HttpGet]
    [Authorize(Policy = Policies.ManageAppointments)]
    [Route("~/api/Data/DeleteAppointment/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteAppointment(Guid id)
    {
        var output = await da.DeleteAppointment(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize(Policy = Policies.ManageAppointments)]
    [Route("~/api/Data/DeleteAppointmentNote/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteAppointmentNote(Guid id)
    {
        var output = await da.DeleteAppointmentNote(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/DeleteAppointmentService/{id}")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> DeleteAppointmentService(Guid id)
    {
        var output = await da.DeleteAppointmentService(id, CurrentUser);
        return Ok(output);
    }

    [HttpGet]
    [Authorize]
    [Route("~/api/Data/GetAppointment/{id}")]
    public async Task<ActionResult<DataObjects.Appointment>> GetAppointment(Guid id)
    {
        var output = await da.GetAppointment(id, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize]
    [Route("~/api/Data/GetAppointments")]
    public async Task<ActionResult<List<DataObjects.Appointment>>> GetAppointments(DataObjects.AppoinmentLoader loader)
    {
        var output = await da.GetAppointments(loader, CurrentUser);
        return Ok(output);
    }

    // {{ModuleItemStart:Invoices}}
    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/GetAppointmentWithInvoices/{id}")]
    public async Task<ActionResult<DataObjects.Appointment>> GetAppointmentWithInvoices(Guid id)
    {
        var output = await da.GetAppointmentWithInvoices(id, CurrentUser);
        return Ok(output);
    }
    // {{ModuleItemEnd:Invoices}}

    [HttpPost]
    [Authorize(Policy = Policies.ManageAppointments)]
    [Route("~/api/Data/SaveAppointment")]
    public async Task<ActionResult<DataObjects.Appointment>> SaveAppointment(DataObjects.Appointment appointment)
    {
        var output = await da.SaveAppointment(appointment, CurrentUser);
        return Ok(output);
    }

    // {{ModuleItemStart:Invoices}}
    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [Route("~/api/Data/SaveAppointmentInvoices")]
    public async Task<ActionResult<DataObjects.Appointment>> SaveAppointmentInvoices(DataObjects.Appointment appointment)
    {
        var output = await da.SaveAppointmentInvoices(appointment, CurrentUser);
        return Ok(output);
    }
    // {{ModuleItemEnd:Invoices}}

    [HttpPost]
    [Authorize(Policy = Policies.ManageAppointments)]
    [Route("~/api/Data/SaveAppointmentNote")]
    public async Task<ActionResult<DataObjects.AppointmentNote>> SaveAppointmentNote(DataObjects.AppointmentNote appointmentNote)
    {
        var output = await da.SaveAppointmentNote(appointmentNote, CurrentUser);
        return Ok(output);
    }

    [HttpPost]
    [Authorize]
    [Route("~/api/Data/UpdateUserAttendance")]
    public async Task<ActionResult<DataObjects.AppointmentAttendanceUpdate>> UpdateUserAttendance(DataObjects.AppointmentAttendanceUpdate update)
    {
        if (CurrentUser.ManageAppointments || CurrentUser.UserId == update.UserId) {
            var output = await da.UpdateUserAttendance(update);
            return Ok(output);
        } else {
            return Unauthorized(_returnCodeAccessDenied);
        }
    }
}
