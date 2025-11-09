using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Hýz Ayarlarý
    [Header("Hareket Ayarlarý")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    // Yönlendirme Ayarlarý (Yeni)
    [Header("Yönlendirme Ayarlarý")]
    [Tooltip("Karakterin dönüþ hýzý.")]
    public float rotationSpeed = 10f;

    private CharacterController characterController;
    private Vector3 moveDirection;
    private float verticalVelocity;

    // Kamera Referansý (Yeni)
    // Karakterin hangi kameraya göre hareket edeceðini bilmek için
    private Transform mainCameraTransform;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Ana Kameranýn Transform bileþenini al (Yeni)
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Sahnede 'MainCamera' etiketi olan bir kamera yok! Kamerayý bulamýyorum.");
        }

        if (characterController == null)
        {
            Debug.LogError("PlayerController scripti için CharacterController bileþeni gerekli!");
            enabled = false;
        }
    }

    void Update()
    {
        // Karakterin hareketini kameraya göre hesaplayacaðýmýz için,
        // Ana Kamerayý bulduðumuzdan emin olmalýyýz.
        if (mainCameraTransform == null) return;

        // 1. Zemin Kontrolü
        if (characterController.isGrounded)
        {
            verticalVelocity = -0.5f;
        }

        // 2. Yatay Hareket Giriþleri (WASD)
        float x = Input.GetAxis("Horizontal"); // A/D veya Sol/Sað Ok
        float z = Input.GetAxis("Vertical");   // W/S veya Yukarý/Aþaðý Ok

        // *** YENÝ KISIM: HAREKET VEKTÖRÜNÜ KAMERAYA GÖRE HESAPLAMA ***

        // Kamera yönünü al, ancak y eksenini (düþey) sýfýrla, böylece sadece yatay hareket yönünü alýrýz.
        Vector3 cameraForward = mainCameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize(); // Vektör uzunluðunu 1'e sabitle (hýzla çarpýlacak)

        Vector3 cameraRight = mainCameraTransform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        // Hareket Vektörü:
        Vector3 desiredMoveDirection = (cameraRight * x) + (cameraForward * z);
        desiredMoveDirection.Normalize();

        // Hýzý uygula
        moveDirection = desiredMoveDirection * moveSpeed;

        // 3. Zýplama Giriþi
        if (Input.GetButtonDown("Jump") && characterController.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 4. Düþey Hareketi (Yer Çekimi)
        verticalVelocity += gravity * Time.deltaTime;

        // Düþey hýzý (Y ekseni) hareket vektörüne uygula
        moveDirection.y = verticalVelocity;

        // 5. Karakteri Hareket Ettir
        characterController.Move(moveDirection * Time.deltaTime);

        // *** YENÝ KISIM: KARAKTERÝ HAREKET YÖNÜNE DÖNDÜRME ***

        // Karakterin yatayda bir hareket giriþi varsa (yani WASD tuþlarýna basýlýyorsa)
        if (desiredMoveDirection.magnitude >= 0.1f)
        {
            // Ýstenen dönüþü (Rotation) hesapla (karakterin gitmek istediði yöne baksýn)
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);

            // Karakteri yumuþakça o yöne döndür
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}