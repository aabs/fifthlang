T4=t4
T4_ARGS=-I=. -r="C:\dev\aabs\5th-related\ast-builder\src\ast-model\bin\Debug\net9.0\ast_model.dll"

SRC_GEN=ast.builders.cs ast.visitors.cs ast.typesystem.cs

%.cs: %.tt %.ttinclude ast.metamodel.ttinclude
	$(T4) $(T4_ARGS) --out=$@ $<

ast2: $(SRC_GEN)
