using ReservasApi.Data.Repositories.Interfaces;
using ReservasApi.Dtos.Request;
using ReservasApi.Dtos.Response;
using ReservasApi.Models;
using ReservasApi.Services.Interfaces;

namespace ReservasApi.Services
{
    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservaRepository;

        public ReservaService(IReservaRepository reservaRepository)
        {
            _reservaRepository = reservaRepository;
        }

        public async Task<IEnumerable<ReservaResponse>> GetAllReservasAsync()
        {
            var reservas = await _reservaRepository.GetAllAsync();
            return reservas.Select(MapToResponse);
        }

        public async Task<IEnumerable<ReservaResponse>> GetReservasByUsuarioAsync(int usuarioId)
        {
            var reservas = await _reservaRepository.GetByUsuarioIdAsync(usuarioId);
            return reservas.Select(MapToResponse);
        }

        public async Task<ReservaResponse?> GetReservaByIdAsync(int id)
        {
            var reserva = await _reservaRepository.GetByIdAsync(id);
            return reserva != null ? MapToResponse(reserva) : null;
        }

        public async Task<ReservaResponse?> GetReservaByIdAndUsuarioAsync(int id, int usuarioId)
        {
            var reserva = await _reservaRepository.GetByIdAndUsuarioIdAsync(id, usuarioId);
            return reserva != null ? MapToResponse(reserva) : null;
        }

        public async Task<ReservaResponse> CreateReservaAsync(CrearReservaRequest request, int usuarioId)
        {
            var reserva = new Reserva
            {
                UsuarioId = usuarioId,
                NombreLocal = request.NombreLocal,
                Fecha = request.Fecha,
                Hora = request.Hora,
                NumeroPersonas = request.NumeroPersonas,
                EstadoReserva = "Por Ir"
            };

            var reservaCreada = await _reservaRepository.CreateAsync(reserva);
            return MapToResponse(reservaCreada);
        }

        public async Task<ReservaResponse?> UpdateReservaAsync(int id, ActualizarReservaRequest request, int usuarioId)
        {
            var reserva = await _reservaRepository.GetByIdAndUsuarioIdAsync(id, usuarioId);
            if (reserva == null) return null;

            // Actualizar solo los campos que no son null
            if (!string.IsNullOrEmpty(request.NombreLocal))
                reserva.NombreLocal = request.NombreLocal;
            
            if (request.Fecha.HasValue)
                reserva.Fecha = request.Fecha.Value;
            
            if (!string.IsNullOrEmpty(request.Hora))
                reserva.Hora = request.Hora;
            
            if (request.NumeroPersonas.HasValue)
                reserva.NumeroPersonas = request.NumeroPersonas.Value;
            
            if (!string.IsNullOrEmpty(request.EstadoReserva))
                reserva.EstadoReserva = request.EstadoReserva;

            var reservaActualizada = await _reservaRepository.UpdateAsync(reserva);
            return MapToResponse(reservaActualizada);
        }

        public async Task<bool> DeleteReservaAsync(int id, int usuarioId)
        {
            var reserva = await _reservaRepository.GetByIdAndUsuarioIdAsync(id, usuarioId);
            if (reserva == null) return false;

            return await _reservaRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ReservaResponse>> GetReservasByEstadoAsync(string estado)
        {
            var reservas = await _reservaRepository.GetByEstadoAsync(estado);
            return reservas.Select(MapToResponse);
        }

        public async Task<IEnumerable<ReservaResponse>> GetReservasByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var reservas = await _reservaRepository.GetByFechaRangoAsync(fechaInicio, fechaFin);
            return reservas.Select(MapToResponse);
        }

        public async Task<ReservaResponse?> CambiarEstadoReservaAsync(int id, string nuevoEstado, int usuarioId)
        {
            var reserva = await _reservaRepository.GetByIdAndUsuarioIdAsync(id, usuarioId);
            if (reserva == null) return null;

            reserva.EstadoReserva = nuevoEstado;
            var reservaActualizada = await _reservaRepository.UpdateAsync(reserva);
            return MapToResponse(reservaActualizada);
        }

        public async Task<bool> PuedeCancelarReservaAsync(int id, int usuarioId)
        {
            var reserva = await _reservaRepository.GetByIdAndUsuarioIdAsync(id, usuarioId);
            if (reserva == null) return false;

            return PuedeCancelar(reserva);
        }

        public async Task<ReservaResponse?> MarcarComoCompletadaAsync(int reservaId)
        {
            var reserva = await _reservaRepository.GetByIdAsync(reservaId);
            if (reserva == null) return null;

            reserva.EstadoReserva = "Visita Completada";
            reserva.FechaActualizacion = DateTime.UtcNow;
            
            var reservaActualizada = await _reservaRepository.UpdateAsync(reserva);
            return MapToResponse(reservaActualizada);
        }

        public async Task ActualizarEstadosAutomaticoAsync()
        {
            var reservasPorIr = await _reservaRepository.GetByEstadoAsync("Por Ir");
            
            foreach (var reserva in reservasPorIr)
            {
                if (DebeMarcarFaltaGrave(reserva) && !reserva.Entregables.Any())
                {
                    reserva.EstadoReserva = "Falta Grave";
                    reserva.FechaActualizacion = DateTime.UtcNow;
                    await _reservaRepository.UpdateAsync(reserva);
                }
            }
        }

        private static bool PuedeCancelar(Reserva reserva)
        {
            if (reserva.EstadoReserva != "Por Ir") return false;
            
            var fechaHoraVisita = reserva.Fecha.Add(TimeSpan.Parse(reserva.Hora));
            return DateTime.Now < fechaHoraVisita;
        }

        private static bool DebeMarcarFaltaGrave(Reserva reserva)
        {
            if (reserva.EstadoReserva != "Por Ir") return false;
            
            var fechaHoraVisita = reserva.Fecha.Add(TimeSpan.Parse(reserva.Hora));
            var limitePlazo = fechaHoraVisita.AddHours(48);
            return DateTime.Now > limitePlazo;
        }

        private static bool EnPeriodoEntrega(Reserva reserva)
        {
            if (reserva.EstadoReserva != "Por Ir") return false;
            
            var fechaHoraVisita = reserva.Fecha.Add(TimeSpan.Parse(reserva.Hora));
            var limitePlazo = fechaHoraVisita.AddHours(48);
            var ahora = DateTime.Now;
            
            return ahora > fechaHoraVisita && ahora <= limitePlazo;
        }

        private static ReservaResponse MapToResponse(Reserva reserva)
        {
            return new ReservaResponse
            {
                Id = reserva.Id,
                UsuarioId = reserva.UsuarioId,
                NombreLocal = reserva.NombreLocal,
                Fecha = reserva.Fecha,
                Hora = reserva.Hora,
                NumeroPersonas = reserva.NumeroPersonas,
                EstadoReserva = reserva.EstadoReserva,
                FechaCreacion = reserva.FechaCreacion,
                FechaActualizacion = reserva.FechaActualizacion,
                Entregables = reserva.Entregables.Select(e => new EntregableResponse
                {
                    Id = e.Id,
                    ReservaId = e.ReservaId,
                    EnlaceTikTok = e.EnlaceTikTok,
                    EnlaceInstagram = e.EnlaceInstagram,
                    CantidadGastada = e.CantidadGastada,
                    FechaCreacion = e.FechaCreacion,
                    FechaActualizacion = e.FechaActualizacion
                }).ToList()
            };
        }
    }
}