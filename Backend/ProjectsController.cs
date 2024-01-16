using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[ApiController]
[Route("[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly GithubService _githubService;

    public ProjectsController(GithubService githubService)
    {
        _githubService = githubService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<dynamic>>> Get()
    {
        var projects = await _githubService.GetProjects();

        string json = JsonConvert.SerializeObject(projects, Formatting.Indented);

        return Ok(json);
    }
}