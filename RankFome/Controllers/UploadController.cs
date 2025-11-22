// =============================================================================
// Controller: UploadController
// =============================================================================
// Responsável pelo upload de arquivos de imagem para o servidor.
// Valida tipo e tamanho do arquivo antes de salvar.
// As imagens são armazenadas na pasta wwwroot/images com nomes únicos (GUID).
// =============================================================================

using Microsoft.AspNetCore.Mvc;

namespace RankFome.Controllers
{
    /// <summary>
    /// Controller responsável pelo gerenciamento de uploads de imagens.
    /// Permite upload de imagens para produtos e lojas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Construtor com injeção do ambiente de hospedagem.
        /// </summary>
        /// <param name="env">Ambiente de hospedagem para acesso ao WebRootPath</param>
        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // =============================================================================
        // POST: api/Upload
        // =============================================================================
        /// <summary>
        /// Realiza o upload de uma imagem para o servidor.
        /// Valida extensão permitida e tamanho máximo de 5MB.
        /// </summary>
        /// <param name="file">Arquivo de imagem enviado via multipart/form-data</param>
        /// <returns>URL pública da imagem salva</returns>
        [HttpPost]
        public async Task<ActionResult<object>> UploadImage(IFormFile file)
        {
            // Valida se arquivo foi enviado
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Arquivo não enviado" });

            // Define extensões de imagem permitidas
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            // Valida extensão do arquivo
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Formato de imagem não suportado. Use: jpg, jpeg, png, gif ou webp" });

            // Valida tamanho máximo (5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Imagem muito grande. Máximo: 5MB" });

            // Gera nome único usando GUID para evitar conflitos
            var fileName = $"{Guid.NewGuid()}{extension}";
            var imagesPath = Path.Combine(_env.WebRootPath, "images");

            // Cria diretório de imagens se não existir
            if (!Directory.Exists(imagesPath))
                Directory.CreateDirectory(imagesPath);

            var filePath = Path.Combine(imagesPath, fileName);

            // Salva arquivo no disco
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Retorna URL pública da imagem
            var imageUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
            return Ok(new { url = imageUrl });
        }
    }
}
