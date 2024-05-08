using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RpgApi.Data;
using RpgApi.Models;

namespace RpgApi.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class DisputasController : ControllerBase
    {
        private readonly DataContext _context;

        public DisputasController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("Arma")]
        public async Task<IActionResult> AtaqueComArmasAsync(Disputa d)
        {
            try
            {
                Personagem? atacante = await _context.TB_PERSONAGENS
                    .Include(p => p.Arma)
                    .FirstOrDefaultAsync(p => p.Id == d.AtacanteId);

                Personagem? oponente = await _context.TB_PERSONAGENS
                    .FirstOrDefaultAsync(p => p.Id == d.OponenteId);

                int dano = atacante!.Arma!.Dano + new Random().Next(atacante.Forca);

                dano = dano - new Random().Next(oponente!.Defesa);

                if (dano > 0)
                    oponente.PontosVida = oponente.PontosVida - (int)dano;
                if (oponente.PontosVida <= 0)
                    d.Narracao = $"{oponente.Nome} foi derrotado.";

                _context.TB_PERSONAGENS.Update(oponente);
                await _context.SaveChangesAsync();

                StringBuilder dados = new StringBuilder();
                dados.AppendFormat($" Atacante: {atacante.Nome}");
                dados.AppendFormat($" Oponente: {oponente.Nome}");
                dados.AppendFormat($" Pontos de vida do atacante: {atacante.PontosVida}");
                dados.AppendFormat($" Pontos de vida do oponente: {oponente.PontosVida}");
                dados.AppendFormat($" Arma utilizada: {atacante.Arma.Nome}");
                dados.AppendFormat($" Dano: {dano}");

                d.Narracao += dados.ToString();
                d.DataDisputa = DateTime.Now;
                _context.TB_DISPUTAS.Add(d);
                _context.SaveChanges();

                return Ok(d);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Habilidade")]
        public async Task<IActionResult> AtaqueComHabilidadeAsync(Disputa d)
        {
            try
            {
                Personagem? atacante = await _context.TB_PERSONAGENS
                    .Include(p => p.PersonagemHabilidades).ThenInclude(pH => pH.Habilidade)
                    .FirstOrDefaultAsync(p => p.Id == d.AtacanteId);

                Personagem? oponente = await _context.TB_PERSONAGENS
                    .FirstOrDefaultAsync(p => p.Id == d.OponenteId);

                PersonagemHabilidade? ph = await _context.TB_PERSONAGENS_HABILIDADES
                    .Include(p => p.Habilidade).FirstOrDefaultAsync(phBusca => phBusca.HabilidadeId == d.HabilidadeId
                    && phBusca.PersonagemId == d.AtacanteId);

                if (ph == null)
                    d.Narracao = $"{atacante!.Nome} não possui essa habilidade";
                else
                {
                    int dano = ph.Habilidade!.Dano + new Random().Next(atacante!.Inteligencia);
                    dano = dano - new Random().Next(oponente!.Defesa);

                    if (dano > 0)
                        oponente.PontosVida = oponente.PontosVida - dano;
                    if (oponente.PontosVida <= 0)
                        d.Narracao += $"{oponente.Nome} foi derrotado!";

                    _context.TB_PERSONAGENS.Update(oponente);
                    await _context.SaveChangesAsync();

                    StringBuilder dados = new StringBuilder();
                    dados.AppendFormat($" Atacante: {atacante.Nome}");
                    dados.AppendFormat($" Oponente: {oponente.Nome}");
                    dados.AppendFormat($" Pontos de vida do atacante: {atacante.PontosVida}");
                    dados.AppendFormat($" Pontos de vida do oponente: {oponente.PontosVida}");
                    dados.AppendFormat($" Arma utilizada: {ph.Habilidade!.Nome}");
                    dados.AppendFormat($" Dano: {dano}");

                    d.Narracao += dados.ToString();
                    d.DataDisputa = DateTime.Now;
                    _context.TB_DISPUTAS.Add(d);
                    _context.SaveChanges();
                }
                return Ok(d);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}