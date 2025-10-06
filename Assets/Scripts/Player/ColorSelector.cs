using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class ColorSelector : MonoBehaviourPunCallbacks
{
    [Header("Color Selection UI")]
    [SerializeField] GameObject colorSelectionPanel;
    [SerializeField] Button[] colorButtons;
    [SerializeField] TMP_Text selectedColorText;
    [SerializeField] Button confirmButton;

    [Header("Available Colors")]
    [SerializeField]
    Color[] availableColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta,
        Color.cyan,
        Color.white,
        Color.gray,
        Color.black
    };

    private Color selectedColor;
    private bool colorConfirmed = false;

    void Start()
    {
        for (int i = 0; i < colorButtons.Length && i < availableColors.Length; i++)
        {
            int index = i;
            colorButtons[i].GetComponent<Image>().color = availableColors[i];
            colorButtons[i].onClick.AddListener(() => SelectColor(index));
        }

        confirmButton.onClick.AddListener(ConfirmColor);

        SelectColor(0);
    }

    void SelectColor(int colorIndex)
    {
        if (colorIndex < 0 || colorIndex >= availableColors.Length) return;

        selectedColor = availableColors[colorIndex];
        selectedColorText.text = $"Color seleccionado: {GetColorName(selectedColor)}";
        selectedColorText.color = selectedColor;

        for (int i = 0; i < colorButtons.Length; i++)
        {
            colorButtons[i].GetComponent<Outline>().enabled = (i == colorIndex);
        }
    }

    string GetColorName(Color color)
    {
        if (color == Color.red) return "ROJO";
        if (color == Color.blue) return "AZUL";
        if (color == Color.green) return "VERDE";
        if (color == Color.yellow) return "AMARILLO";
        if (color == Color.magenta) return "ROSA";
        if (color == Color.cyan) return "CYAN";
        if (color == Color.white) return "BLANCO";
        if (color == Color.gray) return "GRIS";
        if (color == Color.black) return "NEGRO";
        return "COLOR";
    }

    void ConfirmColor()
    {
        colorConfirmed = true;

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["playerColor"] = new Vector3(selectedColor.r, selectedColor.g, selectedColor.b);
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        colorSelectionPanel.SetActive(false);
    }

    public bool IsColorConfirmed()
    {
        return colorConfirmed;
    }

    public Color GetSelectedColor()
    {
        return selectedColor;
    }
}