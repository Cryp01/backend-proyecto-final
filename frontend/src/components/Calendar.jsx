import { useState, useEffect, useCallback } from "react";
import logo from "../assets/logo1.png";

const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5205/api";

const Calendar = () => {
  const [currentDate, setCurrentDate] = useState(new Date());
  const [selectedDate, setSelectedDate] = useState(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [formData, setFormData] = useState({
    hora: "",
    motivo: "",
    telefono: "",
  });
  const [pacienteExistente, setPacienteExistente] = useState(null);
  const [mostrarFormularioPaciente, setMostrarFormularioPaciente] =
    useState(false);
  const [datosNuevoPaciente, setDatosNuevoPaciente] = useState({
    nombre: "",
    direccion: "",
    fechaNacimiento: "",
  });
  const [buscandoPaciente, setBuscandoPaciente] = useState(false);
  const [mostrarCitas, setMostrarCitas] = useState(false);
  const [citas, setCitas] = useState([]);
  const [cargandoCitas, setCargandoCitas] = useState(false);
  const [telefonoBusquedaCitas, setTelefonoBusquedaCitas] = useState("");
  const [pacienteBusquedaCitas, setPacienteBusquedaCitas] = useState(null);
  const [buscandoPacienteCitas, setBuscandoPacienteCitas] = useState(false);
  const [errorBusquedaCitas, setErrorBusquedaCitas] = useState("");
  const [disponibilidadMes, setDisponibilidadMes] = useState(null);
  const [disponibilidadDia, setDisponibilidadDia] = useState(null);
  const [cargandoDisponibilidad, setCargandoDisponibilidad] = useState(false);
  const [mensajeError, setMensajeError] = useState("");
  const [mensajeExito, setMensajeExito] = useState("");

  // Generar horarios disponibles de 8am a 12:30pm cada 30 minutos
  const generateTimeSlots = () => {
    const slots = [];
    for (let hour = 8; hour <= 12; hour++) {
      for (let minute = 0; minute < 60; minute += 30) {
        if (hour === 12 && minute > 30) break; // Última hora es 12:30 PM
        const timeString = `${hour.toString().padStart(2, "0")}:${minute
          .toString()
          .padStart(2, "0")}`;
        const displayTime = formatTime(timeString);
        slots.push({ value: timeString, display: displayTime });
      }
    }
    return slots;
  };

  const formatTime = (time) => {
    const [hours, minutes] = time.split(":");
    const hour = parseInt(hours);
    const ampm = hour >= 12 ? "PM" : "AM";
    const displayHour = hour > 12 ? hour - 12 : hour === 0 ? 12 : hour;
    return `${displayHour}:${minutes} ${ampm}`;
  };

  const timeSlots = generateTimeSlots();

  const monthNames = [
    "Enero",
    "Febrero",
    "Marzo",
    "Abril",
    "Mayo",
    "Junio",
    "Julio",
    "Agosto",
    "Septiembre",
    "Octubre",
    "Noviembre",
    "Diciembre",
  ];

  const dayNames = ["Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"];
  const dayNamesShort = ["D", "L", "M", "X", "J", "V", "S"];

  const getDaysInMonth = (date) => {
    const year = date.getFullYear();
    const month = date.getMonth();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startingDayOfWeek = firstDay.getDay();

    return { daysInMonth, startingDayOfWeek };
  };

  const isToday = (day) => {
    const today = new Date();
    return (
      day === today.getDate() &&
      currentDate.getMonth() === today.getMonth() &&
      currentDate.getFullYear() === today.getFullYear()
    );
  };

  const isPastDate = (day) => {
    const today = new Date();
    today.setHours(0, 0, 0, 0); // Reset time to start of day
    const checkDate = new Date(
      currentDate.getFullYear(),
      currentDate.getMonth(),
      day
    );
    checkDate.setHours(0, 0, 0, 0);
    return checkDate < today;
  };

  const isCurrentMonth = () => {
    const today = new Date();
    return (
      currentDate.getMonth() === today.getMonth() &&
      currentDate.getFullYear() === today.getFullYear()
    );
  };

  const previousMonth = () => {
    setCurrentDate(
      new Date(currentDate.getFullYear(), currentDate.getMonth() - 1)
    );
  };

  const nextMonth = () => {
    setCurrentDate(
      new Date(currentDate.getFullYear(), currentDate.getMonth() + 1)
    );
  };

  const goToToday = () => {
    setCurrentDate(new Date());
  };

  const handleDayClick = async (day) => {
    // No permitir seleccionar días pasados
    if (isPastDate(day)) {
      return;
    }

    // No permitir seleccionar días sin disponibilidad (del backend o porque todas las horas pasaron)
    const diaDisp = getDiaDisponibilidad(day);
    const horasPasaron = todasLasHorasPasaron(day);
    if ((diaDisp && !diaDisp.tieneDisponibilidad) || horasPasaron) {
      return;
    }

    const selected = new Date(
      currentDate.getFullYear(),
      currentDate.getMonth(),
      day
    );
    setSelectedDate(selected);
    setIsModalOpen(true);
    setFormData({ hora: "", motivo: "", telefono: "" });

    // Cargar disponibilidad del día seleccionado
    await cargarDisponibilidadDia(selected);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedDate(null);
    setPacienteExistente(null);
    setMostrarFormularioPaciente(false);
    setDatosNuevoPaciente({
      nombre: "",
      direccion: "",
      fechaNacimiento: "",
    });
    setDisponibilidadDia(null);
    setMensajeError("");
    setMensajeExito("");
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;

    // Validación especial para teléfono
    if (name === "telefono") {
      // Solo permitir números y máximo 10 dígitos
      const onlyNumbers = value.replace(/\D/g, "").slice(0, 10);
      setFormData((prev) => ({
        ...prev,
        [name]: onlyNumbers,
      }));

      // Buscar automáticamente cuando tenga 10 dígitos
      if (onlyNumbers.length === 10) {
        buscarPacientePorTelefono(onlyNumbers);
      } else {
        // Limpiar estado si tiene menos de 10 dígitos
        setPacienteExistente(null);
        setMostrarFormularioPaciente(false);
      }
    } else {
      setFormData((prev) => ({
        ...prev,
        [name]: value,
      }));
    }
  };

  const handleInputChangePaciente = (e) => {
    const { name, value } = e.target;
    setDatosNuevoPaciente((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const buscarPacientePorTelefono = async (telefono) => {
    if (!telefono || telefono.length !== 10) return;

    setBuscandoPaciente(true);
    try {
      const response = await fetch(`${API_URL}/Pacientes/Telefono/${telefono}`);

      if (response.ok) {
        const paciente = await response.json();
        setPacienteExistente(paciente);
        setMostrarFormularioPaciente(false);
        // Llenar el formulario con los datos del paciente
        setDatosNuevoPaciente({
          nombre: paciente.nombre || "",
          direccion: paciente.direccion || "",
          fechaNacimiento: paciente.fechaNacimiento
            ? new Date(paciente.fechaNacimiento).toISOString().split("T")[0]
            : "",
        });
      } else if (response.status === 404) {
        // Paciente no encontrado, limpiar formulario
        setPacienteExistente(null);
        setMostrarFormularioPaciente(true);
        setDatosNuevoPaciente({
          nombre: "",
          direccion: "",
          fechaNacimiento: "",
        });
      } else {
        console.error("Error al buscar paciente");
      }
    } catch (error) {
      console.error("Error en la búsqueda:", error);
    } finally {
      setBuscandoPaciente(false);
    }
  };

  const cargarCitasPorPaciente = async (pacienteId) => {
    setCargandoCitas(true);
    setErrorBusquedaCitas("");
    try {
      const response = await fetch(`${API_URL}/Citas/Paciente/${pacienteId}`);
      if (response.ok) {
        const data = await response.json();
        setCitas(data);
      } else {
        setErrorBusquedaCitas("Error al cargar las citas del paciente");
        setCitas([]);
      }
    } catch (error) {
      console.error("Error al cargar citas:", error);
      setErrorBusquedaCitas("Error al cargar las citas del paciente");
      setCitas([]);
    } finally {
      setCargandoCitas(false);
    }
  };

  const buscarPacienteParaCitas = async () => {
    if (!telefonoBusquedaCitas || telefonoBusquedaCitas.length !== 10) {
      return;
    }

    setBuscandoPacienteCitas(true);
    setErrorBusquedaCitas("");
    setPacienteBusquedaCitas(null);
    setCitas([]);

    try {
      const response = await fetch(
        `${API_URL}/Pacientes/Telefono/${telefonoBusquedaCitas}`
      );

      if (response.ok) {
        const paciente = await response.json();
        setPacienteBusquedaCitas(paciente);
        // Cargar las citas del paciente
        await cargarCitasPorPaciente(paciente.id);
      } else if (response.status === 404) {
        setErrorBusquedaCitas(
          "No se encontró un paciente con ese número de teléfono"
        );
        setPacienteBusquedaCitas(null);
        setCitas([]);
      } else {
        setErrorBusquedaCitas("Error al buscar el paciente");
        setPacienteBusquedaCitas(null);
        setCitas([]);
      }
    } catch (error) {
      console.error("Error en la búsqueda:", error);
      setErrorBusquedaCitas(
        "Error al buscar el paciente. Por favor intenta nuevamente."
      );
      setPacienteBusquedaCitas(null);
      setCitas([]);
    } finally {
      setBuscandoPacienteCitas(false);
    }
  };

  const handleTelefonoBusquedaChange = (e) => {
    const value = e.target.value.replace(/\D/g, "").slice(0, 10);
    setTelefonoBusquedaCitas(value);
    setErrorBusquedaCitas("");

    // Si tiene 10 dígitos, buscar automáticamente
    if (value.length === 10) {
      buscarPacienteParaCitas();
    } else {
      // Limpiar resultados si cambia el teléfono
      setPacienteBusquedaCitas(null);
      setCitas([]);
    }
  };

  const handleMostrarCitas = () => {
    setMostrarCitas(true);
    setTelefonoBusquedaCitas("");
    setPacienteBusquedaCitas(null);
    setCitas([]);
    setErrorBusquedaCitas("");
  };

  const handleCerrarCitas = () => {
    setMostrarCitas(false);
    setTelefonoBusquedaCitas("");
    setPacienteBusquedaCitas(null);
    setCitas([]);
    setErrorBusquedaCitas("");
  };

  const cargarDisponibilidadMes = useCallback(async () => {
    try {
      const fechaConsulta = new Date(
        currentDate.getFullYear(),
        currentDate.getMonth(),
        1
      );
      const response = await fetch(
        `${API_URL}/Citas/Disponibilidad/Mes?fecha=${fechaConsulta.toISOString()}`
      );
      if (response.ok) {
        const data = await response.json();
        setDisponibilidadMes(data);
      } else {
        console.error("Error al cargar disponibilidad del mes");
      }
    } catch (error) {
      console.error("Error al cargar disponibilidad del mes:", error);
    }
  }, [currentDate]);

  const cargarDisponibilidadDia = async (fecha) => {
    setCargandoDisponibilidad(true);
    try {
      const response = await fetch(
        `${API_URL}/Citas/Disponibilidad/Dia?fecha=${fecha.toISOString()}`
      );
      if (response.ok) {
        const data = await response.json();
        setDisponibilidadDia(data);
      } else {
        console.error("Error al cargar disponibilidad del día");
        setDisponibilidadDia(null);
      }
    } catch (error) {
      console.error("Error al cargar disponibilidad del día:", error);
      setDisponibilidadDia(null);
    } finally {
      setCargandoDisponibilidad(false);
    }
  };

  const getDiaDisponibilidad = (day) => {
    if (!disponibilidadMes || !disponibilidadMes.dias) return null;
    const fecha = new Date(
      currentDate.getFullYear(),
      currentDate.getMonth(),
      day
    );
    return disponibilidadMes.dias.find(
      (d) => new Date(d.fecha).toDateString() === fecha.toDateString()
    );
  };

  const todasLasHorasPasaron = (day) => {
    const ahora = new Date();
    const fecha = new Date(
      currentDate.getFullYear(),
      currentDate.getMonth(),
      day
    );

    // Solo verificar si es hoy
    const esHoy =
      fecha.getDate() === ahora.getDate() &&
      fecha.getMonth() === ahora.getMonth() &&
      fecha.getFullYear() === ahora.getFullYear();

    if (!esHoy) return false;

    // Última hora disponible es 12:30 PM (12:30)
    const ultimaHora = new Date(fecha);
    ultimaHora.setHours(12, 30, 0, 0);

    // Si ya pasó la última hora disponible, todas las horas pasaron
    return ahora > ultimaHora;
  };

  // Cargar disponibilidad del mes cuando cambia currentDate
  useEffect(() => {
    cargarDisponibilidadMes();
  }, [cargarDisponibilidadMes]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMensajeError("");
    setMensajeExito("");

    // Validar que se haya seleccionado una hora
    if (!formData.hora) {
      setMensajeError("Por favor selecciona una hora para la cita");
      return;
    }

    // Validar que la fecha y hora no sean en el pasado
    const [hours, minutes] = formData.hora.split(":");
    const fechaHoraCita = new Date(selectedDate);
    fechaHoraCita.setHours(parseInt(hours), parseInt(minutes), 0, 0);
    const ahora = new Date();

    if (fechaHoraCita < ahora) {
      setMensajeError(
        "No se puede crear una cita para una fecha u hora pasada"
      );
      return;
    }

    try {
      let pacienteId = pacienteExistente?.id;

      // Si el paciente no existe, crearlo
      if (!pacienteExistente && mostrarFormularioPaciente) {
        const responsePaciente = await fetch(`${API_URL}/Pacientes`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            nombre: datosNuevoPaciente.nombre,
            telefono: parseFloat(formData.telefono),
            direccion: datosNuevoPaciente.direccion,
            fechaNacimiento: datosNuevoPaciente.fechaNacimiento || null,
          }),
        });

        if (!responsePaciente.ok) {
          let errorMessage = "Error al crear el paciente";
          try {
            const errorData = await responsePaciente.json();
            errorMessage = errorData.message || errorData.error || errorMessage;
          } catch (parseError) {
            console.error("Error al parsear respuesta de error:", parseError);
          }
          setMensajeError(errorMessage);
          return;
        }

        const nuevoPaciente = await responsePaciente.json();
        pacienteId = nuevoPaciente.id;
      }

      // Si el paciente existe pero se modificó, actualizarlo
      if (pacienteExistente) {
        const datosModificados =
          pacienteExistente.nombre !== datosNuevoPaciente.nombre ||
          pacienteExistente.direccion !== datosNuevoPaciente.direccion ||
          (pacienteExistente.fechaNacimiento
            ? new Date(pacienteExistente.fechaNacimiento)
                .toISOString()
                .split("T")[0]
            : "") !== datosNuevoPaciente.fechaNacimiento;

        if (datosModificados) {
          const responseUpdate = await fetch(
            `${API_URL}/Pacientes/${pacienteExistente.id}`,
            {
              method: "PUT",
              headers: {
                "Content-Type": "application/json",
              },
              body: JSON.stringify({
                nombre: datosNuevoPaciente.nombre,
                telefono: parseFloat(formData.telefono),
                direccion: datosNuevoPaciente.direccion,
                fechaNacimiento: datosNuevoPaciente.fechaNacimiento || null,
              }),
            }
          );

          if (!responseUpdate.ok) {
            let errorMessage = "Error al actualizar el paciente";
            try {
              const errorData = await responseUpdate.json();
              errorMessage =
                errorData.message || errorData.error || errorMessage;
            } catch (parseError) {
              console.error("Error al parsear respuesta de error:", parseError);
            }
            setMensajeError(errorMessage);
            return;
          }
        }
      }

      // Combinar fecha y hora
      const [hours, minutes] = formData.hora.split(":");
      const fechaHora = new Date(selectedDate);
      fechaHora.setHours(parseInt(hours), parseInt(minutes), 0, 0);

      // Crear la cita
      const citaData = {
        pacienteId: pacienteId,
        fecha: fechaHora.toISOString(),
        NotaMedica: formData.motivo,
      };

      const responseCita = await fetch(`${API_URL}/Citas`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(citaData),
      });

      if (!responseCita.ok) {
        let errorMessage = "Error al crear la cita";
        try {
          const errorData = await responseCita.json();
          errorMessage = errorData.message || errorData.error || errorMessage;
        } catch (parseError) {
          // Si no se puede parsear el JSON, usar el mensaje por defecto
          console.error("Error al parsear respuesta de error:", parseError);
        }
        setMensajeError(errorMessage);
        return;
      }

      const nuevaCita = await responseCita.json();
      console.log("Cita creada exitosamente:", nuevaCita);

      // Recargar disponibilidad del mes
      await cargarDisponibilidadMes();

      // Cerrar modal después de un breve delay para que se vea el éxito
      setTimeout(() => {
        handleCloseModal();
        setMensajeExito("");
      }, 500);
    } catch (error) {
      console.error("Error al crear la cita:", error);
      setMensajeError(
        error.message ||
          "Error al crear la cita. Por favor, intenta nuevamente."
      );
    }
  };

  const renderCalendarDays = () => {
    const { daysInMonth, startingDayOfWeek } = getDaysInMonth(currentDate);
    const days = [];

    // Empty cells for days before the first day of the month
    for (let i = 0; i < startingDayOfWeek; i++) {
      days.push(
        <div
          key={`empty-${i}`}
          className="aspect-square p-1 sm:p-2 md:p-3 flex items-center justify-center"
        ></div>
      );
    }

    // Days of the month
    for (let day = 1; day <= daysInMonth; day++) {
      const today = isToday(day);
      const isPast = isPastDate(day);
      const diaDisp = getDiaDisponibilidad(day);
      const horasPasaron = todasLasHorasPasaron(day);
      const sinDisponibilidad =
        (diaDisp && !diaDisp.tieneDisponibilidad) || horasPasaron;

      days.push(
        <div
          key={day}
          className="aspect-square p-1 sm:p-2 md:p-3 flex items-center justify-center"
        >
          <div
            onClick={() => handleDayClick(day)}
            className={`w-full h-full rounded-full flex items-center justify-center transition-all duration-300 relative ${
              isPast || sinDisponibilidad
                ? "bg-gray-200 text-gray-400 cursor-not-allowed opacity-50"
                : today
                ? "bg-blue-500 text-white shadow-lg hover:bg-blue-600 cursor-pointer hover:scale-110 active:scale-95"
                : "bg-gray-50 hover:bg-blue-100 text-gray-700 hover:shadow-md cursor-pointer hover:scale-110 active:scale-95"
            }`}
          >
            <span className="text-sm sm:text-base md:text-lg font-semibold">
              {day}
            </span>
            {diaDisp &&
              diaDisp.tieneDisponibilidad &&
              !isPast &&
              !horasPasaron && (
                <span className="absolute bottom-0.5 right-0.5 w-1.5 h-1.5 bg-green-500 rounded-full"></span>
              )}
            {sinDisponibilidad && !isPast && (
              <span className="absolute bottom-0.5 right-0.5 w-1.5 h-1.5 bg-red-500 rounded-full"></span>
            )}
          </div>
        </div>
      );
    }

    return days;
  };

  return (
    <>
      <div className="max-w-3xl mx-auto sm:p-4 sm:mt-4 md:mt-0">
        {/* Page Header */}
        <div className="sm:mb-6 text-center">
          <div className="flex justify-center mb-3 sm:mb-4 p-2 rounded-full bg-white">
            <img
              src={logo}
              alt="SanaDoc Logo"
              className="h-12 sm:h-16 md:h-16 w-auto"
            />
          </div>

          <p className="text-sm sm:text-base text-gray-600">
            Selecciona un día del calendario para agendar tu cita médica
          </p>
        </div>

        <div className="bg-white rounded-2xl sm:rounded-3xl shadow-xl sm:shadow-2xl p-4 sm:p-6 md:p-8">
          {/* Calendar Header */}
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 mb-6 sm:mb-8">
            <h2 className="text-xl sm:text-2xl md:text-3xl font-bold text-gray-800">
              {monthNames[currentDate.getMonth()]} {currentDate.getFullYear()}
            </h2>
            <div className="flex flex-wrap gap-2 sm:gap-3 w-full sm:w-auto">
              <button
                onClick={handleMostrarCitas}
                className="px-3 py-2 sm:px-4 sm:py-2.5 md:px-6 md:py-3 bg-green-500 hover:bg-green-600 text-white rounded-full transition-all duration-300 font-medium text-sm sm:text-base shadow-md hover:shadow-lg active:scale-95 sm:hover:scale-105"
              >
                Ver Citas
              </button>
              <button
                onClick={goToToday}
                className="px-3 py-2 sm:px-4 sm:py-2.5 md:px-6 md:py-3 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-full transition-all duration-300 font-medium text-sm sm:text-base shadow-md hover:shadow-lg active:scale-95 sm:hover:scale-105"
              >
                Hoy
              </button>
              {!isCurrentMonth() && (
                <button
                  onClick={previousMonth}
                  className="flex-1 sm:flex-none px-3 py-2 sm:px-4 sm:py-2.5 md:px-6 md:py-3 bg-blue-500 hover:bg-blue-600 text-white rounded-full transition-all duration-300 font-medium text-sm sm:text-base shadow-md hover:shadow-lg active:scale-95 sm:hover:scale-105"
                >
                  <span className="hidden sm:inline">← Anterior</span>
                  <span className="sm:hidden">←</span>
                </button>
              )}
              <button
                onClick={nextMonth}
                className="flex-1 sm:flex-none px-3 py-2 sm:px-4 sm:py-2.5 md:px-6 md:py-3 bg-blue-500 hover:bg-blue-600 text-white rounded-full transition-all duration-300 font-medium text-sm sm:text-base shadow-md hover:shadow-lg active:scale-95 sm:hover:scale-105"
              >
                <span className="hidden sm:inline">Siguiente →</span>
                <span className="sm:hidden">→</span>
              </button>
            </div>
          </div>

          {/* Day names */}
          <div className="grid grid-cols-7 gap-1 sm:gap-2 mb-2 sm:mb-4">
            {dayNames.map((day, index) => (
              <div
                key={day}
                className="text-center py-2 sm:py-3 md:py-4 font-bold text-gray-600 text-xs sm:text-sm md:text-base lg:text-lg"
              >
                <span className="hidden sm:inline">{day}</span>
                <span className="sm:hidden">{dayNamesShort[index]}</span>
              </div>
            ))}
          </div>

          {/* Calendar grid */}
          <div className="grid grid-cols-7 gap-1 sm:gap-2">
            {renderCalendarDays()}
          </div>

          {/* Leyenda */}
          <div className="mt-6 pt-4 border-t border-gray-200">
            <p className="text-xs text-gray-500 mb-2 font-semibold">
              Disponibilidad:
            </p>
            <div className="flex flex-wrap gap-4 text-xs text-gray-600">
              <div className="flex items-center gap-2">
                <span className="w-3 h-3 bg-green-500 rounded-full"></span>
                <span>Con disponibilidad</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="w-3 h-3 bg-red-500 rounded-full"></span>
                <span>Sin disponibilidad</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="w-3 h-3 bg-gray-300 rounded-full"></span>
                <span>Días pasados</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl sm:rounded-3xl shadow-2xl max-w-4xl w-full p-5 sm:p-6 md:p-8 transform transition-all max-h-[90vh] overflow-y-auto">
            {/* Modal Header */}
            <div className="flex justify-between items-center mb-4 sm:mb-6">
              <h3 className="text-xl sm:text-2xl font-bold text-gray-800">
                Nueva Cita
              </h3>
              <button
                onClick={handleCloseModal}
                className="text-gray-500 hover:text-gray-700 text-3xl leading-none transition-colors w-8 h-8 flex items-center justify-center"
              >
                ×
              </button>
            </div>

            {/* Selected Date Display */}
            <div className="mb-5 sm:mb-6 p-3 sm:p-4 bg-blue-50 rounded-xl sm:rounded-2xl">
              <p className="text-xs sm:text-sm text-gray-600 mb-1">
                Fecha seleccionada
              </p>
              <p className="text-base sm:text-lg font-semibold text-blue-600">
                {selectedDate?.toLocaleDateString("es-ES", {
                  weekday: "long",
                  year: "numeric",
                  month: "long",
                  day: "numeric",
                })}
              </p>
            </div>

            {/* Form */}
            <form onSubmit={handleSubmit} className="space-y-4 sm:space-y-5">
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Columna Izquierda - Datos del Paciente */}
                <div className="space-y-4">
                  <h4 className="text-sm font-bold text-gray-800 uppercase tracking-wide border-b pb-2">
                    Datos del Paciente
                  </h4>

                  <div>
                    <label
                      htmlFor="telefono"
                      className="block text-sm font-semibold text-gray-700 mb-2"
                    >
                      Teléfono *
                    </label>
                    <input
                      type="tel"
                      id="telefono"
                      name="telefono"
                      value={formData.telefono}
                      onChange={handleInputChange}
                      required
                      maxLength="10"
                      className="w-full px-3 py-2.5 sm:px-4 sm:py-3 text-sm sm:text-base border-2 border-gray-200 rounded-xl focus:outline-none focus:border-blue-500 transition-colors"
                      placeholder="Número de teléfono (10 dígitos)"
                    />
                    {buscandoPaciente && (
                      <p className="text-xs text-blue-500 mt-1">
                        Buscando paciente...
                      </p>
                    )}
                    {formData.telefono.length > 0 &&
                      formData.telefono.length < 10 && (
                        <p className="text-xs text-gray-500 mt-1">
                          {10 - formData.telefono.length} dígitos restantes
                        </p>
                      )}
                  </div>

                  {/* Mensaje de estado del paciente */}
                  {pacienteExistente && (
                    <div className="p-3 bg-green-50 border-l-4 border-green-500 rounded">
                      <p className="text-xs font-semibold text-green-700">
                        ✓ Paciente encontrado - Puedes modificar los datos si es
                        necesario
                      </p>
                    </div>
                  )}

                  {mostrarFormularioPaciente && (
                    <div className="p-3 bg-yellow-50 border-l-4 border-yellow-500 rounded">
                      <p className="text-xs font-semibold text-yellow-700">
                        ⚠ Paciente nuevo - Completa los datos
                      </p>
                    </div>
                  )}

                  {/* Formulario del paciente (siempre visible después de buscar) */}
                  {(pacienteExistente || mostrarFormularioPaciente) && (
                    <div className="space-y-3">
                      <div>
                        <label
                          htmlFor="nombre"
                          className="block text-sm font-semibold text-gray-700 mb-2"
                        >
                          Nombre Completo *
                        </label>
                        <input
                          type="text"
                          id="nombre"
                          name="nombre"
                          value={datosNuevoPaciente.nombre}
                          onChange={handleInputChangePaciente}
                          required
                          className="w-full px-3 py-2.5 text-sm border-2 border-gray-200 rounded-xl focus:outline-none focus:border-blue-500 transition-colors"
                          placeholder="Nombre del paciente"
                        />
                      </div>

                      <div>
                        <label
                          htmlFor="direccion"
                          className="block text-sm font-semibold text-gray-700 mb-2"
                        >
                          Dirección
                        </label>
                        <input
                          type="text"
                          id="direccion"
                          name="direccion"
                          value={datosNuevoPaciente.direccion}
                          onChange={handleInputChangePaciente}
                          className="w-full px-3 py-2.5 text-sm border-2 border-gray-200 rounded-xl focus:outline-none focus:border-blue-500 transition-colors"
                          placeholder="Dirección"
                        />
                      </div>

                      <div>
                        <label
                          htmlFor="fechaNacimiento"
                          className="block text-sm font-semibold text-gray-700 mb-2"
                        >
                          Fecha de Nacimiento
                        </label>
                        <input
                          type="date"
                          id="fechaNacimiento"
                          name="fechaNacimiento"
                          value={datosNuevoPaciente.fechaNacimiento}
                          onChange={handleInputChangePaciente}
                          className="w-full px-3 py-2.5 text-sm border-2 border-gray-200 rounded-xl focus:outline-none focus:border-blue-500 transition-colors"
                        />
                      </div>
                    </div>
                  )}
                </div>

                {/* Columna Derecha - Hora y Motivo */}
                <div className="space-y-4">
                  <h4 className="text-sm font-bold text-gray-800 uppercase tracking-wide border-b pb-2">
                    Detalles de la Cita
                  </h4>

                  <div>
                    <label
                      htmlFor="hora"
                      className="block text-sm font-semibold text-gray-700 mb-3"
                    >
                      Hora de la Cita
                      {cargandoDisponibilidad && (
                        <span className="ml-2 text-xs text-blue-500">
                          Cargando disponibilidad...
                        </span>
                      )}
                    </label>
                    <div className="grid grid-cols-2 gap-2 max-h-64 overflow-y-auto pr-2">
                      {timeSlots.map((slot) => {
                        const horario = disponibilidadDia?.horarios?.find(
                          (h) => h.horaFormateada === slot.value
                        );
                        const disponibleDesdeBackend =
                          horario?.disponible !== false;

                        // Verificar si la hora ya pasó (si es hoy)
                        const ahora = new Date();
                        const esHoy =
                          selectedDate &&
                          selectedDate.getDate() === ahora.getDate() &&
                          selectedDate.getMonth() === ahora.getMonth() &&
                          selectedDate.getFullYear() === ahora.getFullYear();

                        let horaPasada = false;
                        if (esHoy && selectedDate) {
                          const [horas, minutos] = slot.value.split(":");
                          const horaCita = new Date(selectedDate);
                          horaCita.setHours(
                            parseInt(horas),
                            parseInt(minutos),
                            0,
                            0
                          );
                          horaPasada = horaCita < ahora;
                        }

                        const disponible =
                          disponibleDesdeBackend && !horaPasada;

                        return (
                          <button
                            key={slot.value}
                            type="button"
                            onClick={() => {
                              if (disponible) {
                                setFormData((prev) => ({
                                  ...prev,
                                  hora: slot.value,
                                }));
                              }
                            }}
                            disabled={!disponible}
                            className={`px-4 py-2.5 text-sm rounded-full transition-all duration-200 font-medium relative ${
                              !disponible
                                ? "bg-gray-200 text-gray-400 cursor-not-allowed line-through opacity-50"
                                : formData.hora === slot.value
                                ? "bg-blue-500 text-white shadow-lg scale-105"
                                : "bg-gray-100 text-gray-700 hover:bg-blue-50 hover:shadow-md"
                            }`}
                          >
                            {slot.display}
                            {!disponible && (
                              <span className="absolute -top-1 -right-1 w-4 h-4 bg-red-500 rounded-full flex items-center justify-center text-white text-xs">
                                ×
                              </span>
                            )}
                          </button>
                        );
                      })}
                    </div>
                    {!formData.hora && (
                      <p className="text-xs text-red-500 mt-2">
                        * Selecciona una hora disponible
                      </p>
                    )}
                  </div>

                  <div>
                    <label
                      htmlFor="motivo"
                      className="block text-sm font-semibold text-gray-700 mb-2"
                    >
                      Motivo de la Cita
                    </label>
                    <textarea
                      id="motivo"
                      name="motivo"
                      value={formData.motivo}
                      onChange={handleInputChange}
                      required
                      rows="4"
                      className="w-full px-3 py-2.5 sm:px-4 sm:py-3 text-sm sm:text-base border-2 border-gray-200 rounded-xl focus:outline-none focus:border-blue-500 transition-colors resize-none"
                      placeholder="Describe el motivo de la cita"
                    />
                  </div>
                </div>
              </div>

              {/* Mensajes de Error y Éxito */}
              {mensajeError && (
                <div className="mt-4 p-3 bg-red-50 border-l-4 border-red-500 rounded-xl">
                  <p className="text-sm text-red-700 font-medium">
                    {mensajeError}
                  </p>
                </div>
              )}
              {mensajeExito && (
                <div className="mt-4 p-3 bg-green-50 border-l-4 border-green-500 rounded-xl">
                  <p className="text-sm text-green-700 font-medium">
                    {mensajeExito}
                  </p>
                </div>
              )}

              {/* Form Actions */}
              <div className="flex gap-2 sm:gap-3 pt-3 sm:pt-4 border-t">
                <button
                  type="button"
                  onClick={handleCloseModal}
                  className="flex-1 px-4 py-2.5 sm:px-6 sm:py-3 text-sm sm:text-base bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-full transition-all duration-300 font-medium shadow-md hover:shadow-lg active:scale-95"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  className="flex-1 px-4 py-2.5 sm:px-6 sm:py-3 text-sm sm:text-base bg-blue-500 hover:bg-blue-600 text-white rounded-full transition-all duration-300 font-medium shadow-md hover:shadow-lg active:scale-95 sm:hover:scale-105"
                >
                  Guardar
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal de Citas */}
      {mostrarCitas && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl sm:rounded-3xl shadow-2xl max-w-4xl w-full p-5 sm:p-6 md:p-8 transform transition-all max-h-[90vh] overflow-y-auto">
            {/* Modal Header */}
            <div className="flex justify-between items-center mb-4 sm:mb-6">
              <h3 className="text-xl sm:text-2xl font-bold text-gray-800">
                Mis Citas
              </h3>
              <button
                onClick={handleCerrarCitas}
                className="text-gray-500 hover:text-gray-700 text-3xl leading-none transition-colors w-8 h-8 flex items-center justify-center"
              >
                ×
              </button>
            </div>

            {/* Buscador de Teléfono */}
            <div className="mb-6">
              <label
                htmlFor="telefonoBusqueda"
                className="block text-sm font-semibold text-gray-700 mb-2"
              >
                Buscar por Número de Teléfono
              </label>
              <div className="flex gap-2">
                <input
                  type="tel"
                  id="telefonoBusqueda"
                  value={telefonoBusquedaCitas}
                  onChange={handleTelefonoBusquedaChange}
                  maxLength="10"
                  className="flex-1 px-4 py-3 border-2 border-gray-200 rounded-xl focus:outline-none focus:border-blue-500 transition-colors"
                  placeholder="Ingresa tu número de teléfono (10 dígitos)"
                />
                <button
                  type="button"
                  onClick={buscarPacienteParaCitas}
                  disabled={
                    !telefonoBusquedaCitas ||
                    telefonoBusquedaCitas.length !== 10
                  }
                  className="px-6 py-3 bg-blue-500 hover:bg-blue-600 disabled:bg-gray-300 disabled:cursor-not-allowed text-white rounded-xl transition-all duration-300 font-medium"
                >
                  Buscar
                </button>
              </div>
              {telefonoBusquedaCitas.length > 0 &&
                telefonoBusquedaCitas.length < 10 && (
                  <p className="text-xs text-gray-500 mt-1">
                    {10 - telefonoBusquedaCitas.length} dígitos restantes
                  </p>
                )}
              {buscandoPacienteCitas && (
                <p className="text-xs text-blue-500 mt-2">
                  Buscando paciente...
                </p>
              )}
              {errorBusquedaCitas && (
                <p className="text-xs text-red-500 mt-2">
                  {errorBusquedaCitas}
                </p>
              )}
            </div>

            {/* Información del Paciente */}
            {pacienteBusquedaCitas && (
              <div className="mb-6 p-4 bg-blue-50 rounded-xl border-l-4 border-blue-500">
                <p className="text-sm text-gray-600 mb-1">
                  Paciente encontrado
                </p>
                <p className="text-lg font-semibold text-blue-700">
                  {pacienteBusquedaCitas.nombre}
                </p>
                <p className="text-sm text-gray-600 mt-1">
                  Teléfono: {pacienteBusquedaCitas.telefono}
                </p>
              </div>
            )}

            {/* Lista de Citas */}
            {pacienteBusquedaCitas && (
              <div className="space-y-4">
                <h4 className="text-lg font-semibold text-gray-800 mb-4">
                  Citas Programadas
                </h4>
                {cargandoCitas ? (
                  <div className="text-center py-8">
                    <p className="text-gray-500">Cargando citas...</p>
                  </div>
                ) : citas.length === 0 ? (
                  <div className="text-center py-8">
                    <p className="text-gray-500">No tienes citas programadas</p>
                  </div>
                ) : (
                  citas.map((cita) => (
                    <div
                      key={cita.id}
                      className="p-4 border-2 border-gray-200 rounded-xl hover:border-blue-300 transition-colors"
                    >
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                          <p className="text-xs text-gray-500 mb-1">Fecha</p>
                          <p className="font-semibold text-gray-800">
                            {new Date(cita.fecha).toLocaleString("es-ES", {
                              dateStyle: "medium",
                              timeStyle: "short",
                            })}
                          </p>
                        </div>
                        <div>
                          <p className="text-xs text-gray-500 mb-1">Estado</p>
                          <span
                            className={`inline-block px-3 py-1 rounded-full text-xs font-semibold ${
                              cita.nombreEstado === "Confirmada"
                                ? "bg-green-100 text-green-700"
                                : cita.nombreEstado === "Pendiente"
                                ? "bg-yellow-100 text-yellow-700"
                                : cita.nombreEstado === "Completada"
                                ? "bg-blue-100 text-blue-700"
                                : cita.nombreEstado === "Cancelada"
                                ? "bg-red-100 text-red-700"
                                : "bg-gray-100 text-gray-700"
                            }`}
                          >
                            {cita.nombreEstado}
                          </span>
                        </div>
                        {cita.notaMedica && (
                          <div className="md:col-span-2">
                            <p className="text-xs text-gray-500 mb-1">
                              Nota Médica
                            </p>
                            <p className="text-gray-700">{cita.notaMedica}</p>
                          </div>
                        )}
                      </div>
                    </div>
                  ))
                )}
              </div>
            )}

            {/* Botón Cerrar */}
            <div className="mt-6 pt-4 border-t">
              <button
                onClick={handleCerrarCitas}
                className="w-full px-6 py-3 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-full transition-all duration-300 font-medium shadow-md hover:shadow-lg active:scale-95"
              >
                Cerrar
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export default Calendar;
