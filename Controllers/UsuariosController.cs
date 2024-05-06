using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RpgApi.Data;
using RpgApi.Models;
using RpgApi.Utils;

namespace RpgApi.Controllers
{
    [Route("[Controller]")]
    public class UsuariosController : ControllerBase
    {

        private readonly DataContext _context;

        public UsuariosController(DataContext context)
        {
            _context = context;
        }

        private async Task<bool> UsuarioExistente(string username)
        {
            if (await _context.TB_USUARIOS.AnyAsync(x => x.Username.ToLower() == username.ToLower())) {
                return true;
            }
            return false;
        }

        // Exercício 2
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try {
                return Ok(await _context.TB_USUARIOS.ToListAsync());
            } catch (System.Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Registrar")]
        public async Task<IActionResult> RegistrarUsuario(Usuario user)
        {
            try
            {
                if (await UsuarioExistente(user.Username))
                    throw new System.Exception("Nome de usuário já existe");

                Criptografia.CriarPasswordHash(user.PasswordString, out byte[] hash, out byte[] salt);
                user.PasswordString = string.Empty;
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
                await _context.TB_USUARIOS.AddAsync(user);

                // Exercício 3
                user.DataAcesso = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(user.Id);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Autenticar")]
        public async Task<IActionResult> AutenticarUsuario(Usuario credenciais) {
            try {
                Usuario? usuario = await _context.TB_USUARIOS.FirstOrDefaultAsync(x => x.Username.ToLower().Equals(credenciais.Username.ToLower()));

                if (usuario == null) {
                    throw new System.Exception("Usuário não encontrado.");
                }
                else if (!Criptografia.VerificarPasswordHash(credenciais.PasswordString, usuario?.PasswordHash, usuario?.PasswordSalt)) {
                    throw new System.Exception("Senha incorreta.");
                }
                else {
                    return Ok(usuario);
                }
            } catch (System.Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        // Exercício 1
        [HttpPut("AlterarSenha")]
        public async Task<IActionResult> AlterarSenha(Usuario credenciais) {
            try {
                Usuario? usuario = await _context.TB_USUARIOS.FirstOrDefaultAsync(x => x.Username.ToLower().Equals(credenciais.Username.ToLower()));

                if (usuario == null) {
                    throw new System.Exception("Usuário não encontrado.");
                }
                else if (!Criptografia.VerificarPasswordHash(credenciais.PasswordString, usuario?.PasswordHash, usuario?.PasswordSalt)) {
                    throw new System.Exception("Senha atual incorreta.");
                }
                else {
                    Criptografia.CriarPasswordHash(credenciais.NewPasswordString, out byte[] hash, out byte[] salt);
                    credenciais.NewPasswordString = string.Empty;
                    usuario.PasswordHash = hash;
                    usuario.PasswordSalt = salt;
                    await _context.SaveChangesAsync();

                    return Ok(usuario);
                }
            } catch (System.Exception ex) {
                return BadRequest(ex.Message);
            }
        }
    }
}