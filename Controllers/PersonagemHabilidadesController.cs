using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RpgApi.Data;
using RpgApi.Models;

namespace RpgApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonagemHabilidadesController : ControllerBase
    {
        private readonly DataContext _context;

        public PersonagemHabilidadesController(DataContext context) {
            _context = context;
        }
        
        [HttpPost]
        public async Task<IActionResult> AddPersonagemHabilidadeAsync(PersonagemHabilidade novoPersonagemHabilidade) {
            try {
                Personagem? personagem = await _context.TB_PERSONAGENS
                    .Include(p => p.Arma)
                    .Include(p => p.PersonagemHabilidades).ThenInclude(ps => ps.Habilidade)
                    .FirstOrDefaultAsync(p => p.Id == novoPersonagemHabilidade.PersonagemId);

                if (personagem == null) throw new System.Exception("Peronsagem não encontrado com o Id informado.");

                Habilidade? habilidade = await _context.TB_HABILIDADES
                    .FirstOrDefaultAsync(h => h.Id == novoPersonagemHabilidade.HabilidadeId);

                if(habilidade == null) throw new System.Exception("Habilidade não encontrada.");

                PersonagemHabilidade ph = new PersonagemHabilidade();
                ph.Personagem = personagem;
                ph.Habilidade = habilidade;
                await _context.TB_PERSONAGENS_HABILIDADES.AddAsync(ph);
                int linhaAfetadas = await _context.SaveChangesAsync();

                return Ok($"{linhaAfetadas} linha(s) afetada(s)");
            } catch (System.Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        // Exercício 5
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetPersonagemHabilidades(int Id) {
            try
            {
                List<PersonagemHabilidade> phs = await _context.TB_PERSONAGENS_HABILIDADES
                    .Include(p => p.Personagem).Include(p => p.Habilidade)
                    .Where(p => p.PersonagemId == Id).ToListAsync();
                return Ok(phs);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Exercício 6
        [HttpGet("GetHabilidades")]
        public async Task<IActionResult> GetAllHabilidades()
        {
            try
            {
                List<Habilidade> habilidades = await _context.TB_HABILIDADES.ToListAsync();
                return Ok(habilidades);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Exercício 7
        [HttpPost("DeletePersonagemHabilidade")]
        public async Task<IActionResult> DeleteAsync(PersonagemHabilidade ph)
        {
            try
            {
                PersonagemHabilidade? toRemove = await _context.TB_PERSONAGENS_HABILIDADES
                    .FirstOrDefaultAsync(phBusca => phBusca.PersonagemId == ph.PersonagemId && phBusca.HabilidadeId == ph.HabilidadeId);

                if (toRemove == null)
                {
                    return BadRequest("Personagem ou Habilidade não encontrados");
                }

                _context.TB_PERSONAGENS_HABILIDADES.Remove(toRemove);
                int linhaAfetadas = await _context.SaveChangesAsync();
                return Ok($"{linhaAfetadas} linha(s) afetada(s)");

            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}