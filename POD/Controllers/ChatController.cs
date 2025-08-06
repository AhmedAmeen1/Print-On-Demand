using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POD.Services;

namespace POD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] QuestionDto dto)
        {
            var answer = await RAGService.ProcessQuery(dto.Question);
            return Ok(new { answer });
        }
    }

    public class QuestionDto
    {
        public string Question { get; set; }
    }

}
