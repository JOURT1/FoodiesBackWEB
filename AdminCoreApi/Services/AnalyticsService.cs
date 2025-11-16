using AdminCoreApi.Dtos.Response;
using AdminCoreApi.Services.Interfaces;

namespace AdminCoreApi.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IReservasApiService _reservasService;
        private readonly IUsersApiService _usersService;
        private readonly IFormularioFoodieApiService _formularioService;

        public AnalyticsService(
            IReservasApiService reservasService,
            IUsersApiService usersService,
            IFormularioFoodieApiService formularioService)
        {
            _reservasService = reservasService;
            _usersService = usersService;
            _formularioService = formularioService;
        }

        public async Task<ResumenGeneralDto> GetResumenGeneralAsync()
        {
            var usuarios = await _usersService.GetAllUsersAsync();
            var reservas = await _reservasService.GetAllReservasAsync();
            var entregables = await _reservasService.GetAllEntregablesAsync();

            var foodies = usuarios.Count(u => u.Roles.Any(r => r.Nombre.ToLower() == "foodie"));
            var restaurantes = usuarios.Count(u => u.Roles.Any(r => r.Nombre.ToLower() == "restaurante"));
            var reservasCompletadas = reservas.Count(r => r.EstadoReserva == "Visita Completada");
            var ingresoTotal = entregables.Sum(e => e.CantidadGastada);

            var reservasPorRestaurante = reservas
                .GroupBy(r => r.NombreLocal)
                .Select(g => new { Nombre = g.Key, Total = g.Count(), Ingreso = g.Sum(r => r.Entregables.Sum(e => e.CantidadGastada)) })
                .OrderByDescending(r => r.Total)
                .ToList();

            var restauranteMasPopular = reservasPorRestaurante.FirstOrDefault();
            var restauranteMenosVisitado = reservasPorRestaurante.LastOrDefault();

            var topRestaurantes = await GetRestaurantesAnalyticsAsync();

            return new ResumenGeneralDto
            {
                TotalUsuarios = usuarios.Count,
                TotalFoodies = foodies,
                TotalRestaurantes = restaurantes,
                TotalReservas = reservas.Count,
                TotalReservasCompletadas = reservasCompletadas,
                IngresoTotalPlataforma = ingresoTotal,
                RestauranteMasPopular = restauranteMasPopular != null ? new RestaurantePopularDto
                {
                    Nombre = restauranteMasPopular.Nombre,
                    TotalReservas = restauranteMasPopular.Total,
                    IngresoTotal = restauranteMasPopular.Ingreso
                } : new RestaurantePopularDto(),
                RestauranteMenosVisitado = restauranteMenosVisitado != null ? new RestaurantePopularDto
                {
                    Nombre = restauranteMenosVisitado.Nombre,
                    TotalReservas = restauranteMenosVisitado.Total,
                    IngresoTotal = restauranteMenosVisitado.Ingreso
                } : new RestaurantePopularDto(),
                TopRestaurantes = topRestaurantes.Take(10).ToList()
            };
        }

        public async Task<List<RestauranteAnalyticsDto>> GetRestaurantesAnalyticsAsync()
        {
            var reservas = await _reservasService.GetAllReservasAsync();
            var entregables = await _reservasService.GetAllEntregablesAsync();

            var restaurantesAgrupados = reservas.GroupBy(r => r.NombreLocal);
            var resultado = new List<RestauranteAnalyticsDto>();

            foreach (var grupo in restaurantesAgrupados)
            {
                var reservasRestaurante = grupo.ToList();
                var totalReservas = reservasRestaurante.Count;
                var completadas = reservasRestaurante.Count(r => r.EstadoReserva == "Visita Completada");
                var pendientes = reservasRestaurante.Count(r => r.EstadoReserva == "Por Ir");
                var faltas = reservasRestaurante.Count(r => r.EstadoReserva == "Falta Grave");

                var reservaIds = reservasRestaurante.Select(r => r.Id).ToList();
                var entregablesRestaurante = entregables.Where(e => reservaIds.Contains(e.ReservaId)).ToList();

                var ingresoTotal = entregablesRestaurante.Sum(e => e.CantidadGastada);
                var ingresoPromedio = totalReservas > 0 ? ingresoTotal / totalReservas : 0;
                var tasaCompletado = totalReservas > 0 ? (double)completadas / totalReservas * 100 : 0;

                // Calcular horas pico
                var horasPico = reservasRestaurante
                    .GroupBy(r => r.Hora)
                    .Select(g => new HoraPicoDto
                    {
                        Hora = g.Key,
                        CantidadReservas = g.Count(),
                        TotalPersonas = g.Sum(r => r.NumeroPersonas)
                    })
                    .OrderByDescending(h => h.CantidadReservas)
                    .Take(5)
                    .ToList();

                resultado.Add(new RestauranteAnalyticsDto
                {
                    NombreRestaurante = grupo.Key,
                    TotalReservas = totalReservas,
                    ReservasCompletadas = completadas,
                    ReservasPendientes = pendientes,
                    FaltasGraves = faltas,
                    IngresoTotal = ingresoTotal,
                    IngresoPromedio = ingresoPromedio,
                    TasaCompletado = Math.Round(tasaCompletado, 2),
                    HorasPico = horasPico
                });
            }

            return resultado.OrderByDescending(r => r.TotalReservas).ToList();
        }

        public async Task<RestauranteAnalyticsDto?> GetRestauranteAnalyticsByNameAsync(string nombreRestaurante)
        {
            var analytics = await GetRestaurantesAnalyticsAsync();
            return analytics.FirstOrDefault(r => r.NombreRestaurante.Equals(nombreRestaurante, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<TendenciaVisitasDto>> GetTendenciasVisitasAsync()
        {
            var reservas = await _reservasService.GetAllReservasAsync();
            var entregables = await _reservasService.GetAllEntregablesAsync();

            var restaurantesAgrupados = reservas.GroupBy(r => r.NombreLocal);
            var resultado = new List<TendenciaVisitasDto>();

            foreach (var grupo in restaurantesAgrupados)
            {
                var tendencia = await CalcularTendenciaRestaurante(grupo.Key, grupo.ToList(), entregables);
                resultado.Add(tendencia);
            }

            return resultado;
        }

        public async Task<TendenciaVisitasDto?> GetTendenciaVisitasByRestauranteAsync(string nombreRestaurante)
        {
            var tendencias = await GetTendenciasVisitasAsync();
            return tendencias.FirstOrDefault(t => t.NombreRestaurante.Equals(nombreRestaurante, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<ComparativaRestaurantesDto> GetComparativaRestaurantesAsync()
        {
            var reservas = await _reservasService.GetAllReservasAsync();
            var entregables = await _reservasService.GetAllEntregablesAsync();

            var restaurantesAgrupados = reservas.GroupBy(r => r.NombreLocal);
            var comparativas = new List<RestauranteComparativoDto>();

            foreach (var grupo in restaurantesAgrupados)
            {
                var reservasRestaurante = grupo.ToList();
                var totalReservas = reservasRestaurante.Count;
                var completadas = reservasRestaurante.Count(r => r.EstadoReserva == "Visita Completada");
                
                var reservaIds = reservasRestaurante.Select(r => r.Id).ToList();
                var entregablesRestaurante = entregables.Where(e => reservaIds.Contains(e.ReservaId)).ToList();
                var ingresoTotal = entregablesRestaurante.Sum(e => e.CantidadGastada);
                
                var tasaCompletado = totalReservas > 0 ? (double)completadas / totalReservas * 100 : 0;
                var promedioPersonas = totalReservas > 0 ? (int)reservasRestaurante.Average(r => r.NumeroPersonas) : 0;

                comparativas.Add(new RestauranteComparativoDto
                {
                    Nombre = grupo.Key,
                    TotalReservas = totalReservas,
                    IngresoTotal = ingresoTotal,
                    TasaCompletado = Math.Round(tasaCompletado, 2),
                    PromedioPersonasPorReserva = promedioPersonas
                });
            }

            return new ComparativaRestaurantesDto
            {
                Restaurantes = comparativas.OrderByDescending(r => r.TotalReservas).ToList()
            };
        }

        private async Task<TendenciaVisitasDto> CalcularTendenciaRestaurante(
            string nombreRestaurante, 
            List<ReservaResponseDto> reservas, 
            List<EntregableResponseDto> todosEntregables)
        {
            // Agrupar por mes
            var visitasPorMes = reservas
                .Where(r => r.EstadoReserva == "Visita Completada")
                .GroupBy(r => new { r.Fecha.Year, r.Fecha.Month })
                .Select(g =>
                {
                    var reservaIds = g.Select(r => r.Id).ToList();
                    var entregablesMes = todosEntregables.Where(e => reservaIds.Contains(e.ReservaId)).ToList();
                    
                    return new VisitaMensualDto
                    {
                        Año = g.Key.Year,
                        Mes = g.Key.Month,
                        NombreMes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                        CantidadVisitas = g.Count(),
                        IngresoTotal = entregablesMes.Sum(e => e.CantidadGastada)
                    };
                })
                .OrderBy(v => v.Año)
                .ThenBy(v => v.Mes)
                .ToList();

            // Calcular predicción usando regresión lineal
            var prediccion = CalcularPrediccionRegresionLineal(visitasPorMes);

            return new TendenciaVisitasDto
            {
                NombreRestaurante = nombreRestaurante,
                VisitasMensuales = visitasPorMes,
                Prediccion = prediccion
            };
        }

        private PrediccionDto CalcularPrediccionRegresionLineal(List<VisitaMensualDto> visitasMensuales)
        {
            if (visitasMensuales.Count < 2)
            {
                var proximoMes = DateTime.Now.AddMonths(1);
                return new PrediccionDto
                {
                    MesSiguiente = proximoMes.Month,
                    AñoSiguiente = proximoMes.Year,
                    NombreMesSiguiente = proximoMes.ToString("MMMM yyyy"),
                    VisitasPredichas = 0,
                    Tendencia = 0,
                    PorcentajeCrecimiento = 0,
                    InterpretacionTendencia = "No hay suficientes datos para predicción"
                };
            }

            // Regresión lineal simple: y = mx + b
            int n = visitasMensuales.Count;
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            for (int i = 0; i < n; i++)
            {
                double x = i + 1; // Índice del mes (1, 2, 3, ...)
                double y = visitasMensuales[i].CantidadVisitas;

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            // Calcular pendiente (m) y ordenada al origen (b)
            double m = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double b = (sumY - m * sumX) / n;

            // Predicción para el siguiente mes
            double xSiguiente = n + 1;
            double visitasPredichas = m * xSiguiente + b;

            // Asegurar que la predicción no sea negativa
            visitasPredichas = Math.Max(0, visitasPredichas);

            // Calcular porcentaje de crecimiento
            var ultimoMes = visitasMensuales.Last();
            double porcentajeCrecimiento = ultimoMes.CantidadVisitas > 0
                ? ((visitasPredichas - ultimoMes.CantidadVisitas) / ultimoMes.CantidadVisitas) * 100
                : 0;

            // Interpretación de la tendencia
            string interpretacion;
            if (m > 0.5)
                interpretacion = "Tendencia de crecimiento fuerte";
            else if (m > 0)
                interpretacion = "Tendencia de crecimiento moderado";
            else if (m > -0.5)
                interpretacion = "Tendencia estable o leve decrecimiento";
            else
                interpretacion = "Tendencia de decrecimiento";

            var proximoMesReal = new DateTime(ultimoMes.Año, ultimoMes.Mes, 1).AddMonths(1);

            return new PrediccionDto
            {
                MesSiguiente = proximoMesReal.Month,
                AñoSiguiente = proximoMesReal.Year,
                NombreMesSiguiente = proximoMesReal.ToString("MMMM yyyy"),
                VisitasPredichas = Math.Round(visitasPredichas, 0),
                Tendencia = Math.Round(m, 2),
                PorcentajeCrecimiento = Math.Round(porcentajeCrecimiento, 2),
                InterpretacionTendencia = interpretacion
            };
        }
    }
}
