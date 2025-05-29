using UnityEngine;
using TMPro; // CAMBIO AQUÍ para usar TextMesh Pro
using UnityEngine.SceneManagement;

public class ControlJuego : MonoBehaviour
{
    public int puntaje = 0;
    public int metaPuntos = 460;
    public float tiempoInicial = 89f; // 1:29 en segundos
    private float tiempoRestante;

    public TMP_Text textoPuntaje; // CAMBIO AQUÍ
    public TMP_Text textoTiempo;  // CAMBIO AQUÍ

    public GameObject pantallaGanaste;
    public GameObject pantallaPerdiste;

    private bool juegoFinalizado = false;

    void Start()
    {
        tiempoRestante = tiempoInicial;
        ActualizarTextoPuntaje();
        ActualizarTextoTiempo();
    }

    void Update()
    {
        if (juegoFinalizado) return;

        tiempoRestante -= Time.deltaTime;
        ActualizarTextoTiempo();

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            FinDelJuego(false); // Perdió
        }

        if (puntaje >= metaPuntos)
        {
            FinDelJuego(true); // Ganó
        }
    }

    public void SumarPuntos(int cantidadCuadrosEliminados)
    {
        puntaje += cantidadCuadrosEliminados * 10;
        ActualizarTextoPuntaje();
    }

    void ActualizarTextoPuntaje()
    {
        textoPuntaje.text = "Puntaje: " + puntaje.ToString();
    }

    void ActualizarTextoTiempo()
    {
        int minutos = Mathf.FloorToInt(tiempoRestante / 60f);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60f);
        textoTiempo.text = "Tiempo: " + minutos.ToString("00") + ":" + segundos.ToString("00");
    }

    void FinDelJuego(bool gano)
    {
        juegoFinalizado = true;
        if (gano)
        {
            pantallaGanaste.SetActive(true);
        }
        else
        {
            pantallaPerdiste.SetActive(true);
        }
    }
}

