# HS_Capstone
한성대 컴퓨터공학부 캡스톤디자인 프로젝트

# 폴더 
에셋 / 팀 이름 /
- 스크립트
- 사운드
- 리소스
  - 플레이어
  - 오브젝트
  - UI
- 프리팹
- 테이블


# 이름 규칙
- 풀더명 : 명사로 쓰기
- 파일명 : 아트/사운드 리소스, 기획 테이블 등등
- 파일종류_리소스종류_세부사항
- ex) Image_Ginsam_Hair
- 클래스 : 파스칼 케이스
- 함수 : 파스칼 케이스
- 변수 : 카멜케이스
- 공백 : 탭

줄여쓰기: 상식선에서 줄이기로 합니다. 

PascalCase (파스칼 케이스)
- 첫글자와 이어지는 단어의 첫글자를 대문자로 표기하는 방법
- 예) GoodPerson, MyKakaoCake, IAmDeveloper
- Pascal 이라는 프로그래밍 언어에서 이러한 표기법을 사용해서 유명해진 방식이야.

camelCase (카멜 케이스)
- 첫단어는 소문자로 표기하지만, 이어지는 단어의 첫글자는 대문자로 표기하는 방법
- 예) goodPerson, myKakaoCake, iAmDeveloper
- 낙타(camel)의 등모양이 볼록한 것에 영감을 얻어서 이렇게 부르기로 했어.
- 코드 작성 규칙들에 대한 자세한 설명은 : [https://velog.io/@rex/%EC%BD%94%EB%93%9C-%EC%9E%91%EC%84%B1-%EA%B7%9C%EC%B9%99%EB%93%A4-Coding-Conventions]

## Developers' Guidelines
이 Repository 는 개발시 GitHub-flow 를 사용합니다.
개발하는 사항이 있는 경우 master에 직접 커밋하는 것이 아닌, `feature/add-subtitle` 형식의 브랜치 이름을 가지고 작업해 주시고,
모든 개발버전의 경우 PR은 `develop` 으로 PR을 걸어 주십시오. `master`는 최종 또는 Release Candidate 등의 정상 동작을 보장하는 주요버전에만 사용됩니다.
PR하기 전에 무조건 말해주세요.

GitHub Flow에 대한 자세한 설명은: [GitHub Flow 공식 문서 (영문)](https://guides.github.com/introduction/flow/) 또는 [GitHub Flow에 관한 이 블로그 포스트](https://ujuc.github.io/2015/12/16/git-flow-github-flow-gitlab-flow/#github-flow) 를 참고해 주시기 바랍니다.

또한 이 레포지토리는 [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) 를 이용한 커밋을 권장합니다.  
Unity Engine 구조상 강제할 수 없으므로, 협조 부탁드립니다.

# 커밋 케이스
- 버그를 고쳐야 할 때: 앞에 fix를 붙이기 ex) UI 표시 버그를 고친다면 fix: indicator bug 
- 중간 피쳐개발을 할 때: 앞에 feat를 붙이기 ex) potion가게 새로운 포션을 개발 한다면 fieat: custom poition develop


