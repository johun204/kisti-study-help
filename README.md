# KISTI 논문전문 보조 프로그램

한국과학기술정보연구원 데이터구축사업의 논문전문 데이터 구축 자동화 프로그램입니다.

## 소개
* 줄바꿈 부분의 띄어쓰기를 맞춤법 검사를 이용해 수정합니다.

  논문 전체를 검사하는 것이 아닌 **줄바꿈 부분만 검사하여 수정**합니다.

* 각주를 자동으로 위 첨자로 수정합니다. ex) 하다.1) -> 하다.<sup>1)</sup>
* <Fig. 1>, <Table 2> 등의 내용을 [Fig. 1], [Table 2]로 수정합니다.

  <표>, <그림> 등 한글이 포함된 경우에는 수정하지 않습니다.
 
* 수식 문자 깨짐 복구 기능.
* 단축키를 이용하여 빠른 입력 가능. (<kbd>F6</kbd> : 복사, <kbd>F7</kbd> : 붙여넣기, <kbd>F8</kbd> : 문단 구분)


![동작 화면](https://raw.githubusercontent.com/johun204/kisti-study-help/main/example.gif)


변환 후 초록색 표시 영역의 띄어쓰기를 반드시 재확인 해주세요.


## 다운로드
* [kisti-study-help.zip](https://github.com/johun204/kisti-study-help/archive/main.zip) 

 폴더 째로 압축 풀어 Release 폴더의 **"논문분석_도우미.exe"** 파일을 실행해 주세요.


## 오류가 발생할 경우
[.Net Framework](https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net48-kor) 최신 버전을 설치하시거나, **백신에서 예외 프로그램에 추가**를 해주시면 됩니다.


## Contribution
  * Created by 조동훈
  * Supported by Korea Institute of Science and Technology Information(KISTI)
