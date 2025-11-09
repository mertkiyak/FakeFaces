using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Hedef (Player)
    [Header("Hedef Ayarlarý")]
    [Tooltip("Kameranýn takip edeceði ve etrafýnda döneceði nesne (Player).")]
    public Transform target;
    [Tooltip("Kameranýn hedeften ne kadar uzakta olacaðý.")]
    public float distance = 5.0f;

    // Kamera Dönüþ Ayarlarý
    [Header("Dönüþ Ayarlarý")]
    [Tooltip("Fare hassasiyeti.")]
    public float mouseSensitivity = 100f;
    [Tooltip("Dönüþün yumuþaklýðý.")]
    public float smoothSpeed = 0.125f;

    // Açý kýsýtlamalarý (Kameranýn ne kadar yukarý ve aþaðý bakabileceði)
    [Tooltip("Kameranýn dikey (Yukarý/Aþaðý) bakabileceði minimum ve maksimum açýlar.")]
    public Vector2 pitchMinMax = new Vector2(-40, 85); // X: Min Pitch, Y: Max Pitch

    private float yaw;   // Yatay açý (Y ekseni etrafýnda dönüþ)
    private float pitch; // Dikey açý (X ekseni etrafýnda dönüþ)

    void Start()
    {
        // Fare imlecini gizle ve oyun penceresinin ortasýnda kilitle
        Cursor.lockState = CursorLockMode.Locked;

        if (target == null)
        {
            Debug.LogError("CameraController scriptinde 'Target' deðiþkeni atanmadý!");
            enabled = false;
        }
    }

    // LateUpdate, tüm Update fonksiyonlarý çalýþtýktan sonra çaðrýlýr.
    // Kamera hareketleri için idealdir, çünkü karakterin hareketini bitirmesini bekler.
    void LateUpdate()
    {
        // 1. Fare Giriþlerini Al
        // Mouse X: Yatay dönüþ (YAW)
        // Mouse Y: Dikey dönüþ (PITCH)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Yaw (Yatay dönüþ) deðerini topla
        yaw += mouseX;

        // Pitch (Dikey dönüþ) deðerini topla ve kýsýtla (clamp et)
        // Mouse Y'yi çýkararak dikey hareketi tersine çeviriyoruz (doðal kamera hareketi için)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        // 2. Dönüþü Hesapla
        // Euler açýlarýný Quaternion'a çevir (Unity'nin dönüþ birimi)
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0);

        // 3. Konumu Hesapla
        // Kameranýn nihai hedef konumu: (Hedefin Konumu) + (Dönüþün Geri Yönü * Uzaklýk)
        Vector3 desiredPosition = target.position - targetRotation * Vector3.forward * distance;

        // 4. Konumu Yumuþakça Güncelle (Smooth Transition)
        // Konumu anýnda deðil, yavaþça istenen konuma doðru kaydýr
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // 5. Kamerayý Hedefe Doðru Döndür
        // Kameranýn her zaman hedefe bakmasýný saðla
        transform.LookAt(target.position);

        // --- Ek Ýyileþtirme (Opsiyonel): Karakterin Dönüþü ---
        // Bu kýsým, karakteri kameranýn baktýðý yöne doðru döndürür (3. þahýs oyunlar için yaygýn)
        // PlayerController'ý kullanýyorsanýz, bu kodu PlayerController'a taþýmanýz gerekebilir!
        // target.rotation = Quaternion.Euler(0, yaw, 0); 
    }
}