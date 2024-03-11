using Data.Contract.Request;
using Data.Managers;
using Data.Repository.EntityFilters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


public class UserController : Controller
{
    private readonly IUserManager _userManager;

    public UserController(IUserManager userManager)
    {
        _userManager = userManager;
    }

    public async Task<ActionResult> Index()
    {
        var users = await _userManager.SearchAsync(new UserFilter());
        return View(users);
    }

    public async Task<ActionResult> Details(string id)
    {
        var user = await _userManager.GetAsync(new UserFilter { EntityIdAsStringEqualTo = id });
        return View(user);
    }

    public ActionResult Create()
    {
        UserRequest model = new UserRequest();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Save(UserRequest model)
    {
        if (ModelState.IsValid)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                await _userManager.SaveAsync(model);
            }
            else
            {
                await _userManager.UpdateAsync(model);
            }
            return RedirectToAction(nameof(Index));
        }

        string viewName = string.IsNullOrEmpty(model.Id) ? "Create" : "Edit";
        return View(viewName, model);
    }


    public async Task<ActionResult> Edit(string id)
    {
        var user = await _userManager.GetAsync(new UserFilter { EntityIdAsStringEqualTo = id });
        UserRequest model = new UserRequest()
        {
            FirstName = user.FirstName,
            Id = user.Id,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Name = user.Name,
            Email = user.Email,
        };
        return View(model);
    }


    public async Task<ActionResult> Delete(string id)
    {
        var user = await _userManager.GetAsync(new UserFilter { EntityIdAsStringEqualTo = id });
        return View(user);
    }

    [HttpPost]
    public async Task<ActionResult> DeleteConfirmed(string id)
    {
        await _userManager.DeleteAsync(new UserFilter { EntityIdAsStringEqualTo = id });
        return RedirectToAction("Index");
    }


}
