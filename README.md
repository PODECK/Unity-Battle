## Setup

### Required Unity Settings

Unity 프로젝트를 Git으로 관리하기 전에 아래 설정을 먼저 맞춰주세요.

- `Edit > Project Settings > Editor`
- `Version Control / Mode` → `Visible Meta Files`
- `Asset Serialization / Mode` → `Force Text`

이 설정은 `.meta` 파일과 Unity 에셋 변경 사항을 안정적으로 추적하기 위해 필요합니다.

### Git LFS

대용량 에셋 관리를 위해 Git LFS를 사용합니다.

```bash
git lfs install
```

필요한 바이너리 파일은 `.gitattributes`에서 추적하도록 설정합니다.

### Repository Rules

- `Library/`, `Temp/`, `Build/`, `Builds/`는 커밋하지 않습니다.
- `.meta` 파일은 반드시 함께 커밋합니다.
- 큰 이미지, 모델, 오디오 파일은 Git LFS로 관리합니다.
- Unity 버전은 팀 내에서 동일하게 맞춥니다.

### WebGL Build

이 프로젝트는 WebGL 배포를 전제로 합니다.

- `File > Build Settings`
- `WebGL` 선택
- `Switch Platform` 실행

빌드 산출물은 기본적으로 레포에 직접 커밋하지 않고, 배포용 아티팩트로 관리합니다.
