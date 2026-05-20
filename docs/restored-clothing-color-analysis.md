# Restored Clothing Color Analysis

This report maps restored clothing crate textures to the nearest vanilla linen cloth colors by pixel comparison.

Method:
- resolve the actual wearable texture PNG(s) used by the restored clothing piece
- compare each non-transparent pixel to the vanilla linen swatches
- total the closest cloth-color matches

This is a first-pass art heuristic, not a lore-perfect material parser.

## hand

### `clothes-hand-clockmaker-wristguard`

- Texture analysis: no direct cloth texture path resolved

### `clothes-hand-commoner-gloves`

- Texture analysis: no direct cloth texture path resolved

### `clothes-hand-tailor-gloves`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/hand/tailor-gloves.png`
- Closest cloth colors: `white` 95.5%, `plain` 4.5%
- Suggested restore cloth: `2x cloth-white`


## head

### `clothes-head-midsummer`

- Texture analysis: no direct cloth texture path resolved

### `clothes-head-popinjay`

- Texture analysis: no direct cloth texture path resolved

### `clothes-head-ruralhunter`

- Texture analysis: no direct cloth texture path resolved


## lowerbody

### `clothes-lowerbody-beggar`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/lowerbody/beggar.png`
- Closest cloth colors: `black` 46.1%, `brown` 35.7%, `gray` 18.2%
- Suggested restore cloth: `1x cloth-black` + `1x cloth-brown`

### `clothes-lowerbody-centurion`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/lowerbody/centurion.png`
- Closest cloth colors: `brown` 98.2%, `gray` 1.8%
- Suggested restore cloth: `2x cloth-brown`

### `clothes-lowerbody-farmhand`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/lowerbody/farmhand.png`
- Closest cloth colors: `green` 77.5%, `black` 11.6%, `brown` 10.9%
- Suggested restore cloth: `2x cloth-green`

### `clothes-lowerbody-popinjay`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/lowerbody/popinjay.png`
- Closest cloth colors: `red` 64.7%, `brown` 26.0%, `yellow` 7.8%, `plain` 1.5%
- Suggested restore cloth: `1x cloth-red` + `1x cloth-brown`

### `clothes-lowerbody-wanderer`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/lowerbody/wanderer.png`
- Closest cloth colors: `gray` 87.4%, `plain` 10.0%, `brown` 2.6%
- Suggested restore cloth: `2x cloth-gray`

### `clothes-lowerbody-warrior`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/lowerbody/warrior.png`
- Closest cloth colors: `black` 28.6%, `purple` 22.0%, `green` 21.8%, `brown` 14.7%
- Suggested restore cloth: `1x cloth-black` + `1x cloth-purple`


## shoulder

### `clothes-shoulder-clockmaker-apron`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/shoulder/clockmaker-apron.png`
- Closest cloth colors: `red` 46.1%, `brown` 29.5%, `black` 16.3%, `orange` 6.2%
- Suggested restore cloth: `1x cloth-red` + `1x cloth-brown`

### `clothes-shoulder-patchwork`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/shoulder/patchwork.png`
- Closest cloth colors: `black` 64.4%, `brown` 23.8%, `green` 4.4%, `gray` 3.4%
- Suggested restore cloth: `1x cloth-black` + `1x cloth-brown`

### `clothes-shoulder-ruralhunter`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/shoulder/ruralhunter.png`
- Closest cloth colors: `brown` 59.4%, `black` 38.3%, `orange` 2.2%, `plain` 0.1%
- Suggested restore cloth: `1x cloth-brown` + `1x cloth-black`

### `clothes-shoulder-wanderer`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/shoulder/wanderer.png`
- Closest cloth colors: `brown` 53.3%, `black` 29.1%, `red` 5.2%, `plain` 4.2%
- Suggested restore cloth: `1x cloth-brown` + `1x cloth-black`


## upperbody

### `clothes-upperbody-beggar`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/upperbody/beggar.png`
- Closest cloth colors: `gray` 78.1%, `plain` 21.9%
- Suggested restore cloth: `2x cloth-gray`

### `clothes-upperbody-centurion`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/upperbody/centurion.png`
- Closest cloth colors: `red` 69.9%, `brown` 30.1%
- Suggested restore cloth: `1x cloth-red` + `1x cloth-brown`

### `clothes-upperbody-clockmaker-shirt`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/upperbody/clockmaker-shirt.png`
- Closest cloth colors: `plain` 65.1%, `gray` 11.5%, `black` 7.5%, `brown` 6.0%
- Suggested restore cloth: `2x cloth-plain`

### `clothes-upperbody-farmhand`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/upperbody/farmhand.png`
- Closest cloth colors: `plain` 50.4%, `gray` 42.5%, `red` 2.1%, `orange` 2.0%
- Suggested restore cloth: `1x cloth-plain` + `1x cloth-gray`

### `clothes-upperbody-midsummer`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/upperbody/midsummer.png`
- Closest cloth colors: `white` 86.6%, `plain` 13.3%, `gray` 0.1%
- Suggested restore cloth: `2x cloth-white`

### `clothes-upperbody-popinjay`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/upperbody/popinjay.png`
- Closest cloth colors: `black` 91.1%, `red` 5.1%, `yellow` 2.6%, `orange` 0.9%
- Suggested restore cloth: `2x cloth-black`

### `clothes-upperbody-ruralfarmer`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/upperbody/ruralfarmer.png`
- Closest cloth colors: `white` 49.1%, `plain` 40.8%, `orange` 8.5%, `gray` 1.5%
- Suggested restore cloth: `1x cloth-white` + `1x cloth-plain`

### `clothes-upperbody-ruralhunter`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/upperbody/ruralhunter.png`
- Closest cloth colors: `plain` 64.6%, `white` 20.8%, `gray` 8.9%, `brown` 3.7%
- Suggested restore cloth: `2x cloth-plain`

### `clothes-upperbody-wanderer`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/upperbody/wanderer.png`
- Closest cloth colors: `brown` 22.2%, `plain` 20.3%, `blue` 19.9%, `white` 15.9%
- Suggested restore cloth: `2x cloth-brown`

### `clothes-upperbody-warrior`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/upperbody/warrior.png`
- Closest cloth colors: `brown` 34.1%, `gray` 17.5%, `black` 16.6%, `red` 13.7%
- Suggested restore cloth: `2x cloth-brown`


## upperbodyover

### `clothes-upperbodyover-clockmaker-tunic`

- Texture file(s): `/Applications/Vintage Story 1.22.app/assets/survival/textures/entity/humanoid/seraphclothes/upperbodyover/clockmaker-tunic.png`
- Closest cloth colors: `purple` 50.9%, `black` 34.3%, `gray` 6.3%, `brown` 5.1%
- Suggested restore cloth: `1x cloth-purple` + `1x cloth-black`


## waist

### `clothes-waist-beggar`

- Texture analysis: no direct cloth texture path resolved

### `clothes-waist-centurion`

- Texture analysis: no direct cloth texture path resolved

### `clothes-waist-farmhand`

- Texture analysis: no direct cloth texture path resolved

### `clothes-waist-midsummer`

- Texture analysis: no direct cloth texture path resolved

### `clothes-waist-popinjay`

- Texture analysis: no direct cloth texture path resolved

### `clothes-waist-ruralfarmer`

- Texture analysis: no direct cloth texture path resolved

### `clothes-waist-ruralhunter`

- Texture analysis: no direct cloth texture path resolved

### `clothes-waist-wanderer`

- Texture analysis: no direct cloth texture path resolved

### `clothes-waist-warrior`

- Texture analysis: no direct cloth texture path resolved
