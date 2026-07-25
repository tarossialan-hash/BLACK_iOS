from PIL import Image
import os

source = r"C:\Users\Alan Tarossi\Desktop\Projetos\BLACK_iOS\BlackIOS\Resources\assets\logo_black.png"
dest_dir = r"C:\Users\Alan Tarossi\Desktop\Projetos\BLACK_iOS\BlackIOS\Assets.xcassets\AppIcon.appiconset"

sizes = [20, 29, 40, 58, 60, 76, 80, 87, 120, 152, 167, 180, 1024]

# Ensure the image exists and open it
img = Image.open(source).convert("RGBA")

for size in sizes:
    bg = Image.new('RGBA', (size, size), (0, 0, 0, 255))
    
    # Scale down the original logo to fit inside the square, with some padding
    target_size = int(size * 0.8)
    if target_size == 0:
        target_size = 1
        
    img_resized = img.copy()
    img_resized.thumbnail((target_size, target_size), Image.Resampling.LANCZOS)
    
    # Paste the resized logo into the center of the background
    offset = ((size - img_resized.width) // 2, (size - img_resized.height) // 2)
    bg.paste(img_resized, offset, img_resized)
    
    out_path = os.path.join(dest_dir, f"Icon{size}.png")
    bg.save(out_path)

print("Icons generated.")
