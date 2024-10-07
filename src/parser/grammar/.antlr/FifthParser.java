// Generated from c:\dev\aabs\fifthlang\fifth.parser\grammar\Fifth.g4 by ANTLR 4.9.2
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class FifthParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.9.2", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		ALIAS=1, AS=2, CLASS=3, ELSE=4, FALSE=5, IF=6, LIST=7, NEW=8, RETURN=9, 
		USE=10, TRUE=11, WHILE=12, WITH=13, AMP=14, AND=15, ASSIGN=16, BAR=17, 
		CLOSEBRACE=18, CLOSEBRACK=19, CLOSEPAREN=20, COLON=21, COMMA=22, DIVIDE=23, 
		DOT=24, EQ=25, GEN=26, GEQ=27, GT=28, HASH=29, LAMBDASEP=30, LEQ=31, LT=32, 
		MINUS=33, NEQ=34, NOT=35, OPENBRACE=36, OPENBRACK=37, OPENPAREN=38, OR=39, 
		PERCENT=40, PLUS=41, POWER=42, QMARK=43, SEMICOLON=44, TIMES=45, UNDERSCORE=46, 
		IDENTIFIER=47, STRING=48, INT=49, FLOAT=50, WS=51;
	public static final int
		RULE_fifth = 0, RULE_function_call = 1, RULE_module_import = 2, RULE_module_name = 3, 
		RULE_packagename = 4, RULE_class_definition = 5, RULE_property_declaration = 6, 
		RULE_member_access = 7, RULE_type_initialiser = 8, RULE_type_name = 9, 
		RULE_type_property_init = 10, RULE_function_declaration = 11, RULE_function_body = 12, 
		RULE_function_name = 13, RULE_function_type = 14, RULE_formal_parameters = 15, 
		RULE_function_args = 16, RULE_v1_parameter_declaration = 17, RULE_v1_type_destructuring_paramdecl = 18, 
		RULE_v1_property_binding = 19, RULE_variable_constraint = 20, RULE_v1_parameter_type = 21, 
		RULE_v1_parameter_name = 22, RULE_paramdecl = 23, RULE_param_name = 24, 
		RULE_param_type = 25, RULE_destructuring_decl = 26, RULE_destructure_binding = 27, 
		RULE_block = 28, RULE_statement = 29, RULE_var_decl = 30, RULE_explist = 31, 
		RULE_exp = 32, RULE_truth_value = 33, RULE_identifier_chain = 34, RULE_var_name = 35, 
		RULE_alias = 36, RULE_iri = 37, RULE_qNameIri = 38, RULE_absoluteIri = 39, 
		RULE_iri_query_param = 40, RULE_list_type_signature = 41, RULE_list = 42, 
		RULE_list_body = 43, RULE_list_literal = 44, RULE_list_comprehension = 45, 
		RULE_list_comp_generator = 46, RULE_list_comp_constraint = 47;
	private static String[] makeRuleNames() {
		return new String[] {
			"fifth", "function_call", "module_import", "module_name", "packagename", 
			"class_definition", "property_declaration", "member_access", "type_initialiser", 
			"type_name", "type_property_init", "function_declaration", "function_body", 
			"function_name", "function_type", "formal_parameters", "function_args", 
			"v1_parameter_declaration", "v1_type_destructuring_paramdecl", "v1_property_binding", 
			"variable_constraint", "v1_parameter_type", "v1_parameter_name", "paramdecl", 
			"param_name", "param_type", "destructuring_decl", "destructure_binding", 
			"block", "statement", "var_decl", "explist", "exp", "truth_value", "identifier_chain", 
			"var_name", "alias", "iri", "qNameIri", "absoluteIri", "iri_query_param", 
			"list_type_signature", "list", "list_body", "list_literal", "list_comprehension", 
			"list_comp_generator", "list_comp_constraint"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'alias'", "'as'", "'class'", "'else'", "'false'", "'if'", "'list'", 
			"'new'", "'return'", "'use'", "'true'", "'while'", "'with'", "'&'", "'&&'", 
			"'='", "'|'", "'}'", "']'", "')'", "':'", "','", "'/'", "'.'", "'=='", 
			"'<-'", "'>='", "'>'", "'#'", "'=>'", "'<='", "'<'", "'-'", "'!='", "'!'", 
			"'{'", "'['", "'('", "'||'", "'%'", "'+'", "'^'", "'?'", "';'", "'*'", 
			"'_'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "ALIAS", "AS", "CLASS", "ELSE", "FALSE", "IF", "LIST", "NEW", "RETURN", 
			"USE", "TRUE", "WHILE", "WITH", "AMP", "AND", "ASSIGN", "BAR", "CLOSEBRACE", 
			"CLOSEBRACK", "CLOSEPAREN", "COLON", "COMMA", "DIVIDE", "DOT", "EQ", 
			"GEN", "GEQ", "GT", "HASH", "LAMBDASEP", "LEQ", "LT", "MINUS", "NEQ", 
			"NOT", "OPENBRACE", "OPENBRACK", "OPENPAREN", "OR", "PERCENT", "PLUS", 
			"POWER", "QMARK", "SEMICOLON", "TIMES", "UNDERSCORE", "IDENTIFIER", "STRING", 
			"INT", "FLOAT", "WS"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}

	@Override
	public String getGrammarFileName() { return "Fifth.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public FifthParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	public static class FifthContext extends ParserRuleContext {
		public Function_declarationContext function_declaration;
		public List<Function_declarationContext> functions = new ArrayList<Function_declarationContext>();
		public Class_definitionContext class_definition;
		public List<Class_definitionContext> classes = new ArrayList<Class_definitionContext>();
		public List<Module_importContext> module_import() {
			return getRuleContexts(Module_importContext.class);
		}
		public Module_importContext module_import(int i) {
			return getRuleContext(Module_importContext.class,i);
		}
		public List<AliasContext> alias() {
			return getRuleContexts(AliasContext.class);
		}
		public AliasContext alias(int i) {
			return getRuleContext(AliasContext.class,i);
		}
		public List<Function_declarationContext> function_declaration() {
			return getRuleContexts(Function_declarationContext.class);
		}
		public Function_declarationContext function_declaration(int i) {
			return getRuleContext(Function_declarationContext.class,i);
		}
		public List<Class_definitionContext> class_definition() {
			return getRuleContexts(Class_definitionContext.class);
		}
		public Class_definitionContext class_definition(int i) {
			return getRuleContext(Class_definitionContext.class,i);
		}
		public FifthContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_fifth; }
	}

	public final FifthContext fifth() throws RecognitionException {
		FifthContext _localctx = new FifthContext(_ctx, getState());
		enterRule(_localctx, 0, RULE_fifth);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(99);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==USE) {
				{
				{
				setState(96);
				module_import();
				}
				}
				setState(101);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(105);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==ALIAS) {
				{
				{
				setState(102);
				alias();
				}
				}
				setState(107);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(112);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==CLASS || _la==IDENTIFIER) {
				{
				setState(110);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case IDENTIFIER:
					{
					setState(108);
					((FifthContext)_localctx).function_declaration = function_declaration();
					((FifthContext)_localctx).functions.add(((FifthContext)_localctx).function_declaration);
					}
					break;
				case CLASS:
					{
					setState(109);
					((FifthContext)_localctx).class_definition = class_definition();
					((FifthContext)_localctx).classes.add(((FifthContext)_localctx).class_definition);
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(114);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Function_callContext extends ParserRuleContext {
		public Function_nameContext function_name() {
			return getRuleContext(Function_nameContext.class,0);
		}
		public TerminalNode OPENPAREN() { return getToken(FifthParser.OPENPAREN, 0); }
		public List<ExpContext> exp() {
			return getRuleContexts(ExpContext.class);
		}
		public ExpContext exp(int i) {
			return getRuleContext(ExpContext.class,i);
		}
		public TerminalNode CLOSEPAREN() { return getToken(FifthParser.CLOSEPAREN, 0); }
		public List<TerminalNode> COMMA() { return getTokens(FifthParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(FifthParser.COMMA, i);
		}
		public Function_callContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_function_call; }
	}

	public final Function_callContext function_call() throws RecognitionException {
		Function_callContext _localctx = new Function_callContext(_ctx, getState());
		enterRule(_localctx, 2, RULE_function_call);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(115);
			function_name();
			setState(116);
			match(OPENPAREN);
			setState(117);
			exp(0);
			setState(122);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(118);
				match(COMMA);
				setState(119);
				exp(0);
				}
				}
				setState(124);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(125);
			match(CLOSEPAREN);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Module_importContext extends ParserRuleContext {
		public TerminalNode USE() { return getToken(FifthParser.USE, 0); }
		public List<Module_nameContext> module_name() {
			return getRuleContexts(Module_nameContext.class);
		}
		public Module_nameContext module_name(int i) {
			return getRuleContext(Module_nameContext.class,i);
		}
		public TerminalNode SEMICOLON() { return getToken(FifthParser.SEMICOLON, 0); }
		public List<TerminalNode> COMMA() { return getTokens(FifthParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(FifthParser.COMMA, i);
		}
		public Module_importContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_module_import; }
	}

	public final Module_importContext module_import() throws RecognitionException {
		Module_importContext _localctx = new Module_importContext(_ctx, getState());
		enterRule(_localctx, 4, RULE_module_import);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(127);
			match(USE);
			setState(128);
			module_name();
			setState(133);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(129);
				match(COMMA);
				setState(130);
				module_name();
				}
				}
				setState(135);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(136);
			match(SEMICOLON);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Module_nameContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public Module_nameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_module_name; }
	}

	public final Module_nameContext module_name() throws RecognitionException {
		Module_nameContext _localctx = new Module_nameContext(_ctx, getState());
		enterRule(_localctx, 6, RULE_module_name);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(138);
			match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class PackagenameContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public PackagenameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_packagename; }
	}

	public final PackagenameContext packagename() throws RecognitionException {
		PackagenameContext _localctx = new PackagenameContext(_ctx, getState());
		enterRule(_localctx, 8, RULE_packagename);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(140);
			match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Class_definitionContext extends ParserRuleContext {
		public Token name;
		public Function_declarationContext function_declaration;
		public List<Function_declarationContext> functions = new ArrayList<Function_declarationContext>();
		public Property_declarationContext property_declaration;
		public List<Property_declarationContext> properties = new ArrayList<Property_declarationContext>();
		public TerminalNode CLASS() { return getToken(FifthParser.CLASS, 0); }
		public TerminalNode OPENBRACE() { return getToken(FifthParser.OPENBRACE, 0); }
		public TerminalNode CLOSEBRACE() { return getToken(FifthParser.CLOSEBRACE, 0); }
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public List<Function_declarationContext> function_declaration() {
			return getRuleContexts(Function_declarationContext.class);
		}
		public Function_declarationContext function_declaration(int i) {
			return getRuleContext(Function_declarationContext.class,i);
		}
		public List<Property_declarationContext> property_declaration() {
			return getRuleContexts(Property_declarationContext.class);
		}
		public Property_declarationContext property_declaration(int i) {
			return getRuleContext(Property_declarationContext.class,i);
		}
		public Class_definitionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_class_definition; }
	}

	public final Class_definitionContext class_definition() throws RecognitionException {
		Class_definitionContext _localctx = new Class_definitionContext(_ctx, getState());
		enterRule(_localctx, 10, RULE_class_definition);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(142);
			match(CLASS);
			setState(143);
			((Class_definitionContext)_localctx).name = match(IDENTIFIER);
			setState(144);
			match(OPENBRACE);
			setState(149);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==IDENTIFIER) {
				{
				setState(147);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,6,_ctx) ) {
				case 1:
					{
					setState(145);
					((Class_definitionContext)_localctx).function_declaration = function_declaration();
					((Class_definitionContext)_localctx).functions.add(((Class_definitionContext)_localctx).function_declaration);
					}
					break;
				case 2:
					{
					setState(146);
					((Class_definitionContext)_localctx).property_declaration = property_declaration();
					((Class_definitionContext)_localctx).properties.add(((Class_definitionContext)_localctx).property_declaration);
					}
					break;
				}
				}
				setState(151);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(152);
			match(CLOSEBRACE);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Property_declarationContext extends ParserRuleContext {
		public Token name;
		public Token type;
		public TerminalNode COLON() { return getToken(FifthParser.COLON, 0); }
		public TerminalNode SEMICOLON() { return getToken(FifthParser.SEMICOLON, 0); }
		public List<TerminalNode> IDENTIFIER() { return getTokens(FifthParser.IDENTIFIER); }
		public TerminalNode IDENTIFIER(int i) {
			return getToken(FifthParser.IDENTIFIER, i);
		}
		public Property_declarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_property_declaration; }
	}

	public final Property_declarationContext property_declaration() throws RecognitionException {
		Property_declarationContext _localctx = new Property_declarationContext(_ctx, getState());
		enterRule(_localctx, 12, RULE_property_declaration);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(154);
			((Property_declarationContext)_localctx).name = match(IDENTIFIER);
			setState(155);
			match(COLON);
			setState(156);
			((Property_declarationContext)_localctx).type = match(IDENTIFIER);
			setState(157);
			match(SEMICOLON);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Member_accessContext extends ParserRuleContext {
		public TerminalNode DOT() { return getToken(FifthParser.DOT, 0); }
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public Member_accessContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_member_access; }
	}

	public final Member_accessContext member_access() throws RecognitionException {
		Member_accessContext _localctx = new Member_accessContext(_ctx, getState());
		enterRule(_localctx, 14, RULE_member_access);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(159);
			match(DOT);
			setState(160);
			match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Type_initialiserContext extends ParserRuleContext {
		public Type_nameContext typename;
		public Type_property_initContext type_property_init;
		public List<Type_property_initContext> properties = new ArrayList<Type_property_initContext>();
		public TerminalNode OPENBRACE() { return getToken(FifthParser.OPENBRACE, 0); }
		public TerminalNode CLOSEBRACE() { return getToken(FifthParser.CLOSEBRACE, 0); }
		public Type_nameContext type_name() {
			return getRuleContext(Type_nameContext.class,0);
		}
		public List<Type_property_initContext> type_property_init() {
			return getRuleContexts(Type_property_initContext.class);
		}
		public Type_property_initContext type_property_init(int i) {
			return getRuleContext(Type_property_initContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(FifthParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(FifthParser.COMMA, i);
		}
		public Type_initialiserContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_type_initialiser; }
	}

	public final Type_initialiserContext type_initialiser() throws RecognitionException {
		Type_initialiserContext _localctx = new Type_initialiserContext(_ctx, getState());
		enterRule(_localctx, 16, RULE_type_initialiser);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(162);
			((Type_initialiserContext)_localctx).typename = type_name();
			setState(163);
			match(OPENBRACE);
			setState(164);
			((Type_initialiserContext)_localctx).type_property_init = type_property_init();
			((Type_initialiserContext)_localctx).properties.add(((Type_initialiserContext)_localctx).type_property_init);
			setState(169);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(165);
				match(COMMA);
				setState(166);
				((Type_initialiserContext)_localctx).type_property_init = type_property_init();
				((Type_initialiserContext)_localctx).properties.add(((Type_initialiserContext)_localctx).type_property_init);
				}
				}
				setState(171);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(172);
			match(CLOSEBRACE);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Type_nameContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public Type_nameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_type_name; }
	}

	public final Type_nameContext type_name() throws RecognitionException {
		Type_nameContext _localctx = new Type_nameContext(_ctx, getState());
		enterRule(_localctx, 18, RULE_type_name);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(174);
			match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Type_property_initContext extends ParserRuleContext {
		public Var_nameContext var_name() {
			return getRuleContext(Var_nameContext.class,0);
		}
		public TerminalNode ASSIGN() { return getToken(FifthParser.ASSIGN, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public Type_property_initContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_type_property_init; }
	}

	public final Type_property_initContext type_property_init() throws RecognitionException {
		Type_property_initContext _localctx = new Type_property_initContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_type_property_init);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(176);
			var_name();
			setState(177);
			match(ASSIGN);
			setState(178);
			exp(0);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Function_declarationContext extends ParserRuleContext {
		public Function_nameContext name;
		public Function_argsContext args;
		public Function_typeContext result_type;
		public Function_bodyContext body;
		public TerminalNode COLON() { return getToken(FifthParser.COLON, 0); }
		public Function_nameContext function_name() {
			return getRuleContext(Function_nameContext.class,0);
		}
		public Function_argsContext function_args() {
			return getRuleContext(Function_argsContext.class,0);
		}
		public Function_typeContext function_type() {
			return getRuleContext(Function_typeContext.class,0);
		}
		public Function_bodyContext function_body() {
			return getRuleContext(Function_bodyContext.class,0);
		}
		public Function_declarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_function_declaration; }
	}

	public final Function_declarationContext function_declaration() throws RecognitionException {
		Function_declarationContext _localctx = new Function_declarationContext(_ctx, getState());
		enterRule(_localctx, 22, RULE_function_declaration);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(180);
			((Function_declarationContext)_localctx).name = function_name();
			setState(181);
			((Function_declarationContext)_localctx).args = function_args();
			setState(182);
			match(COLON);
			setState(183);
			((Function_declarationContext)_localctx).result_type = function_type();
			setState(184);
			((Function_declarationContext)_localctx).body = function_body();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Function_bodyContext extends ParserRuleContext {
		public BlockContext block() {
			return getRuleContext(BlockContext.class,0);
		}
		public Function_bodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_function_body; }
	}

	public final Function_bodyContext function_body() throws RecognitionException {
		Function_bodyContext _localctx = new Function_bodyContext(_ctx, getState());
		enterRule(_localctx, 24, RULE_function_body);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(186);
			block();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Function_nameContext extends ParserRuleContext {
		public Identifier_chainContext identifier_chain() {
			return getRuleContext(Identifier_chainContext.class,0);
		}
		public Function_nameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_function_name; }
	}

	public final Function_nameContext function_name() throws RecognitionException {
		Function_nameContext _localctx = new Function_nameContext(_ctx, getState());
		enterRule(_localctx, 26, RULE_function_name);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(188);
			identifier_chain();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Function_typeContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public Function_typeContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_function_type; }
	}

	public final Function_typeContext function_type() throws RecognitionException {
		Function_typeContext _localctx = new Function_typeContext(_ctx, getState());
		enterRule(_localctx, 28, RULE_function_type);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(190);
			match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Formal_parametersContext extends ParserRuleContext {
		public List<ParamdeclContext> paramdecl() {
			return getRuleContexts(ParamdeclContext.class);
		}
		public ParamdeclContext paramdecl(int i) {
			return getRuleContext(ParamdeclContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(FifthParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(FifthParser.COMMA, i);
		}
		public Formal_parametersContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_formal_parameters; }
	}

	public final Formal_parametersContext formal_parameters() throws RecognitionException {
		Formal_parametersContext _localctx = new Formal_parametersContext(_ctx, getState());
		enterRule(_localctx, 30, RULE_formal_parameters);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(192);
			paramdecl();
			setState(197);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(193);
				match(COMMA);
				setState(194);
				paramdecl();
				}
				}
				setState(199);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Function_argsContext extends ParserRuleContext {
		public TerminalNode OPENPAREN() { return getToken(FifthParser.OPENPAREN, 0); }
		public TerminalNode CLOSEPAREN() { return getToken(FifthParser.CLOSEPAREN, 0); }
		public Formal_parametersContext formal_parameters() {
			return getRuleContext(Formal_parametersContext.class,0);
		}
		public Function_argsContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_function_args; }
	}

	public final Function_argsContext function_args() throws RecognitionException {
		Function_argsContext _localctx = new Function_argsContext(_ctx, getState());
		enterRule(_localctx, 32, RULE_function_args);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(200);
			match(OPENPAREN);
			setState(202);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==IDENTIFIER) {
				{
				setState(201);
				formal_parameters();
				}
			}

			setState(204);
			match(CLOSEPAREN);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class V1_parameter_declarationContext extends ParserRuleContext {
		public V1_parameter_declarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_v1_parameter_declaration; }
	 
		public V1_parameter_declarationContext() { }
		public void copyFrom(V1_parameter_declarationContext ctx) {
			super.copyFrom(ctx);
		}
	}
	public static class ParamDeclWithTypeDestructureContext extends V1_parameter_declarationContext {
		public V1_type_destructuring_paramdeclContext v1_type_destructuring_paramdecl() {
			return getRuleContext(V1_type_destructuring_paramdeclContext.class,0);
		}
		public ParamDeclWithTypeDestructureContext(V1_parameter_declarationContext ctx) { copyFrom(ctx); }
	}
	public static class ParamDeclContext extends V1_parameter_declarationContext {
		public V1_parameter_nameContext v1_parameter_name() {
			return getRuleContext(V1_parameter_nameContext.class,0);
		}
		public TerminalNode COLON() { return getToken(FifthParser.COLON, 0); }
		public V1_parameter_typeContext v1_parameter_type() {
			return getRuleContext(V1_parameter_typeContext.class,0);
		}
		public Variable_constraintContext variable_constraint() {
			return getRuleContext(Variable_constraintContext.class,0);
		}
		public ParamDeclContext(V1_parameter_declarationContext ctx) { copyFrom(ctx); }
	}

	public final V1_parameter_declarationContext v1_parameter_declaration() throws RecognitionException {
		V1_parameter_declarationContext _localctx = new V1_parameter_declarationContext(_ctx, getState());
		enterRule(_localctx, 34, RULE_v1_parameter_declaration);
		int _la;
		try {
			setState(213);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,12,_ctx) ) {
			case 1:
				_localctx = new ParamDeclContext(_localctx);
				enterOuterAlt(_localctx, 1);
				{
				setState(206);
				v1_parameter_name();
				setState(207);
				match(COLON);
				setState(208);
				v1_parameter_type();
				setState(210);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==BAR) {
					{
					setState(209);
					variable_constraint();
					}
				}

				}
				break;
			case 2:
				_localctx = new ParamDeclWithTypeDestructureContext(_localctx);
				enterOuterAlt(_localctx, 2);
				{
				setState(212);
				v1_type_destructuring_paramdecl();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class V1_type_destructuring_paramdeclContext extends ParserRuleContext {
		public V1_property_bindingContext v1_property_binding;
		public List<V1_property_bindingContext> bindings = new ArrayList<V1_property_bindingContext>();
		public V1_parameter_nameContext v1_parameter_name() {
			return getRuleContext(V1_parameter_nameContext.class,0);
		}
		public TerminalNode COLON() { return getToken(FifthParser.COLON, 0); }
		public V1_parameter_typeContext v1_parameter_type() {
			return getRuleContext(V1_parameter_typeContext.class,0);
		}
		public TerminalNode OPENBRACE() { return getToken(FifthParser.OPENBRACE, 0); }
		public TerminalNode CLOSEBRACE() { return getToken(FifthParser.CLOSEBRACE, 0); }
		public List<V1_property_bindingContext> v1_property_binding() {
			return getRuleContexts(V1_property_bindingContext.class);
		}
		public V1_property_bindingContext v1_property_binding(int i) {
			return getRuleContext(V1_property_bindingContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(FifthParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(FifthParser.COMMA, i);
		}
		public V1_type_destructuring_paramdeclContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_v1_type_destructuring_paramdecl; }
	}

	public final V1_type_destructuring_paramdeclContext v1_type_destructuring_paramdecl() throws RecognitionException {
		V1_type_destructuring_paramdeclContext _localctx = new V1_type_destructuring_paramdeclContext(_ctx, getState());
		enterRule(_localctx, 36, RULE_v1_type_destructuring_paramdecl);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(215);
			v1_parameter_name();
			setState(216);
			match(COLON);
			setState(217);
			v1_parameter_type();
			setState(218);
			match(OPENBRACE);
			setState(219);
			((V1_type_destructuring_paramdeclContext)_localctx).v1_property_binding = v1_property_binding();
			((V1_type_destructuring_paramdeclContext)_localctx).bindings.add(((V1_type_destructuring_paramdeclContext)_localctx).v1_property_binding);
			setState(224);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(220);
				match(COMMA);
				setState(221);
				((V1_type_destructuring_paramdeclContext)_localctx).v1_property_binding = v1_property_binding();
				((V1_type_destructuring_paramdeclContext)_localctx).bindings.add(((V1_type_destructuring_paramdeclContext)_localctx).v1_property_binding);
				}
				}
				setState(226);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(227);
			match(CLOSEBRACE);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class V1_property_bindingContext extends ParserRuleContext {
		public Var_nameContext bound_variable_name;
		public Var_nameContext property_name;
		public TerminalNode COLON() { return getToken(FifthParser.COLON, 0); }
		public List<Var_nameContext> var_name() {
			return getRuleContexts(Var_nameContext.class);
		}
		public Var_nameContext var_name(int i) {
			return getRuleContext(Var_nameContext.class,i);
		}
		public Variable_constraintContext variable_constraint() {
			return getRuleContext(Variable_constraintContext.class,0);
		}
		public V1_property_bindingContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_v1_property_binding; }
	}

	public final V1_property_bindingContext v1_property_binding() throws RecognitionException {
		V1_property_bindingContext _localctx = new V1_property_bindingContext(_ctx, getState());
		enterRule(_localctx, 38, RULE_v1_property_binding);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(229);
			((V1_property_bindingContext)_localctx).bound_variable_name = var_name();
			setState(230);
			match(COLON);
			setState(231);
			((V1_property_bindingContext)_localctx).property_name = var_name();
			setState(233);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==BAR) {
				{
				setState(232);
				variable_constraint();
				}
			}

			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Variable_constraintContext extends ParserRuleContext {
		public ExpContext constraint;
		public TerminalNode BAR() { return getToken(FifthParser.BAR, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public Variable_constraintContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_variable_constraint; }
	}

	public final Variable_constraintContext variable_constraint() throws RecognitionException {
		Variable_constraintContext _localctx = new Variable_constraintContext(_ctx, getState());
		enterRule(_localctx, 40, RULE_variable_constraint);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(235);
			match(BAR);
			setState(236);
			((Variable_constraintContext)_localctx).constraint = exp(0);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class V1_parameter_typeContext extends ParserRuleContext {
		public Identifier_chainContext identifier_chain() {
			return getRuleContext(Identifier_chainContext.class,0);
		}
		public V1_parameter_typeContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_v1_parameter_type; }
	}

	public final V1_parameter_typeContext v1_parameter_type() throws RecognitionException {
		V1_parameter_typeContext _localctx = new V1_parameter_typeContext(_ctx, getState());
		enterRule(_localctx, 42, RULE_v1_parameter_type);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(238);
			identifier_chain();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class V1_parameter_nameContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public V1_parameter_nameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_v1_parameter_name; }
	}

	public final V1_parameter_nameContext v1_parameter_name() throws RecognitionException {
		V1_parameter_nameContext _localctx = new V1_parameter_nameContext(_ctx, getState());
		enterRule(_localctx, 44, RULE_v1_parameter_name);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(240);
			match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ParamdeclContext extends ParserRuleContext {
		public Param_nameContext param_name() {
			return getRuleContext(Param_nameContext.class,0);
		}
		public TerminalNode COLON() { return getToken(FifthParser.COLON, 0); }
		public Param_typeContext param_type() {
			return getRuleContext(Param_typeContext.class,0);
		}
		public Destructuring_declContext destructuring_decl() {
			return getRuleContext(Destructuring_declContext.class,0);
		}
		public Variable_constraintContext variable_constraint() {
			return getRuleContext(Variable_constraintContext.class,0);
		}
		public ParamdeclContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_paramdecl; }
	}

	public final ParamdeclContext paramdecl() throws RecognitionException {
		ParamdeclContext _localctx = new ParamdeclContext(_ctx, getState());
		enterRule(_localctx, 46, RULE_paramdecl);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(242);
			param_name();
			setState(243);
			match(COLON);
			setState(244);
			param_type();
			setState(249);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,16,_ctx) ) {
			case 1:
				{
				setState(246);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==BAR) {
					{
					setState(245);
					variable_constraint();
					}
				}

				}
				break;
			case 2:
				{
				setState(248);
				destructuring_decl();
				}
				break;
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Param_nameContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public Param_nameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_param_name; }
	}

	public final Param_nameContext param_name() throws RecognitionException {
		Param_nameContext _localctx = new Param_nameContext(_ctx, getState());
		enterRule(_localctx, 48, RULE_param_name);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(251);
			match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Param_typeContext extends ParserRuleContext {
		public Identifier_chainContext identifier_chain() {
			return getRuleContext(Identifier_chainContext.class,0);
		}
		public Param_typeContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_param_type; }
	}

	public final Param_typeContext param_type() throws RecognitionException {
		Param_typeContext _localctx = new Param_typeContext(_ctx, getState());
		enterRule(_localctx, 50, RULE_param_type);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(253);
			identifier_chain();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Destructuring_declContext extends ParserRuleContext {
		public Destructure_bindingContext destructure_binding;
		public List<Destructure_bindingContext> bindings = new ArrayList<Destructure_bindingContext>();
		public TerminalNode OPENBRACE() { return getToken(FifthParser.OPENBRACE, 0); }
		public TerminalNode CLOSEBRACE() { return getToken(FifthParser.CLOSEBRACE, 0); }
		public List<Destructure_bindingContext> destructure_binding() {
			return getRuleContexts(Destructure_bindingContext.class);
		}
		public Destructure_bindingContext destructure_binding(int i) {
			return getRuleContext(Destructure_bindingContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(FifthParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(FifthParser.COMMA, i);
		}
		public Destructuring_declContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_destructuring_decl; }
	}

	public final Destructuring_declContext destructuring_decl() throws RecognitionException {
		Destructuring_declContext _localctx = new Destructuring_declContext(_ctx, getState());
		enterRule(_localctx, 52, RULE_destructuring_decl);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(255);
			match(OPENBRACE);
			setState(256);
			((Destructuring_declContext)_localctx).destructure_binding = destructure_binding();
			((Destructuring_declContext)_localctx).bindings.add(((Destructuring_declContext)_localctx).destructure_binding);
			setState(261);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(257);
				match(COMMA);
				setState(258);
				((Destructuring_declContext)_localctx).destructure_binding = destructure_binding();
				((Destructuring_declContext)_localctx).bindings.add(((Destructuring_declContext)_localctx).destructure_binding);
				}
				}
				setState(263);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(264);
			match(CLOSEBRACE);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Destructure_bindingContext extends ParserRuleContext {
		public List<Param_nameContext> param_name() {
			return getRuleContexts(Param_nameContext.class);
		}
		public Param_nameContext param_name(int i) {
			return getRuleContext(Param_nameContext.class,i);
		}
		public TerminalNode COLON() { return getToken(FifthParser.COLON, 0); }
		public Destructuring_declContext destructuring_decl() {
			return getRuleContext(Destructuring_declContext.class,0);
		}
		public Destructure_bindingContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_destructure_binding; }
	}

	public final Destructure_bindingContext destructure_binding() throws RecognitionException {
		Destructure_bindingContext _localctx = new Destructure_bindingContext(_ctx, getState());
		enterRule(_localctx, 54, RULE_destructure_binding);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(266);
			param_name();
			setState(267);
			match(COLON);
			setState(268);
			param_name();
			setState(270);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==OPENBRACE) {
				{
				setState(269);
				destructuring_decl();
				}
			}

			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class BlockContext extends ParserRuleContext {
		public TerminalNode OPENBRACE() { return getToken(FifthParser.OPENBRACE, 0); }
		public TerminalNode CLOSEBRACE() { return getToken(FifthParser.CLOSEBRACE, 0); }
		public List<StatementContext> statement() {
			return getRuleContexts(StatementContext.class);
		}
		public StatementContext statement(int i) {
			return getRuleContext(StatementContext.class,i);
		}
		public List<TerminalNode> SEMICOLON() { return getTokens(FifthParser.SEMICOLON); }
		public TerminalNode SEMICOLON(int i) {
			return getToken(FifthParser.SEMICOLON, i);
		}
		public BlockContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_block; }
	}

	public final BlockContext block() throws RecognitionException {
		BlockContext _localctx = new BlockContext(_ctx, getState());
		enterRule(_localctx, 56, RULE_block);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(272);
			match(OPENBRACE);
			setState(278);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << FALSE) | (1L << IF) | (1L << NEW) | (1L << RETURN) | (1L << TRUE) | (1L << WHILE) | (1L << WITH) | (1L << MINUS) | (1L << NOT) | (1L << OPENBRACK) | (1L << OPENPAREN) | (1L << IDENTIFIER) | (1L << STRING) | (1L << INT) | (1L << FLOAT))) != 0)) {
				{
				{
				setState(273);
				statement();
				setState(274);
				match(SEMICOLON);
				}
				}
				setState(280);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(281);
			match(CLOSEBRACE);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class StatementContext extends ParserRuleContext {
		public StatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_statement; }
	 
		public StatementContext() { }
		public void copyFrom(StatementContext ctx) {
			super.copyFrom(ctx);
		}
	}
	public static class SWhileContext extends StatementContext {
		public ExpContext condition;
		public BlockContext looppart;
		public TerminalNode WHILE() { return getToken(FifthParser.WHILE, 0); }
		public TerminalNode OPENPAREN() { return getToken(FifthParser.OPENPAREN, 0); }
		public TerminalNode CLOSEPAREN() { return getToken(FifthParser.CLOSEPAREN, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public BlockContext block() {
			return getRuleContext(BlockContext.class,0);
		}
		public SWhileContext(StatementContext ctx) { copyFrom(ctx); }
	}
	public static class SWithContext extends StatementContext {
		public TerminalNode WITH() { return getToken(FifthParser.WITH, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public BlockContext block() {
			return getRuleContext(BlockContext.class,0);
		}
		public SWithContext(StatementContext ctx) { copyFrom(ctx); }
	}
	public static class SAssignmentContext extends StatementContext {
		public Var_nameContext var_name() {
			return getRuleContext(Var_nameContext.class,0);
		}
		public TerminalNode ASSIGN() { return getToken(FifthParser.ASSIGN, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public SAssignmentContext(StatementContext ctx) { copyFrom(ctx); }
	}
	public static class SBareExpressionContext extends StatementContext {
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public SBareExpressionContext(StatementContext ctx) { copyFrom(ctx); }
	}
	public static class SVarDeclContext extends StatementContext {
		public Var_declContext decl;
		public Var_declContext var_decl() {
			return getRuleContext(Var_declContext.class,0);
		}
		public TerminalNode ASSIGN() { return getToken(FifthParser.ASSIGN, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public SVarDeclContext(StatementContext ctx) { copyFrom(ctx); }
	}
	public static class SReturnContext extends StatementContext {
		public TerminalNode RETURN() { return getToken(FifthParser.RETURN, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public SReturnContext(StatementContext ctx) { copyFrom(ctx); }
	}
	public static class SIfElseContext extends StatementContext {
		public ExpContext condition;
		public BlockContext ifpart;
		public BlockContext elsepart;
		public TerminalNode IF() { return getToken(FifthParser.IF, 0); }
		public TerminalNode OPENPAREN() { return getToken(FifthParser.OPENPAREN, 0); }
		public TerminalNode CLOSEPAREN() { return getToken(FifthParser.CLOSEPAREN, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public List<BlockContext> block() {
			return getRuleContexts(BlockContext.class);
		}
		public BlockContext block(int i) {
			return getRuleContext(BlockContext.class,i);
		}
		public TerminalNode ELSE() { return getToken(FifthParser.ELSE, 0); }
		public SIfElseContext(StatementContext ctx) { copyFrom(ctx); }
	}

	public final StatementContext statement() throws RecognitionException {
		StatementContext _localctx = new StatementContext(_ctx, getState());
		enterRule(_localctx, 58, RULE_statement);
		int _la;
		try {
			setState(314);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,22,_ctx) ) {
			case 1:
				_localctx = new SIfElseContext(_localctx);
				enterOuterAlt(_localctx, 1);
				{
				setState(283);
				match(IF);
				setState(284);
				match(OPENPAREN);
				setState(285);
				((SIfElseContext)_localctx).condition = exp(0);
				setState(286);
				match(CLOSEPAREN);
				setState(287);
				((SIfElseContext)_localctx).ifpart = block();
				setState(290);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==ELSE) {
					{
					setState(288);
					match(ELSE);
					setState(289);
					((SIfElseContext)_localctx).elsepart = block();
					}
				}

				}
				break;
			case 2:
				_localctx = new SWhileContext(_localctx);
				enterOuterAlt(_localctx, 2);
				{
				setState(292);
				match(WHILE);
				setState(293);
				match(OPENPAREN);
				setState(294);
				((SWhileContext)_localctx).condition = exp(0);
				setState(295);
				match(CLOSEPAREN);
				setState(296);
				((SWhileContext)_localctx).looppart = block();
				}
				break;
			case 3:
				_localctx = new SWithContext(_localctx);
				enterOuterAlt(_localctx, 3);
				{
				setState(298);
				match(WITH);
				setState(299);
				exp(0);
				setState(300);
				block();
				}
				break;
			case 4:
				_localctx = new SVarDeclContext(_localctx);
				enterOuterAlt(_localctx, 4);
				{
				setState(302);
				((SVarDeclContext)_localctx).decl = var_decl();
				setState(305);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==ASSIGN) {
					{
					setState(303);
					match(ASSIGN);
					setState(304);
					exp(0);
					}
				}

				}
				break;
			case 5:
				_localctx = new SAssignmentContext(_localctx);
				enterOuterAlt(_localctx, 5);
				{
				setState(307);
				var_name();
				setState(308);
				match(ASSIGN);
				setState(309);
				exp(0);
				}
				break;
			case 6:
				_localctx = new SReturnContext(_localctx);
				enterOuterAlt(_localctx, 6);
				{
				setState(311);
				match(RETURN);
				setState(312);
				exp(0);
				}
				break;
			case 7:
				_localctx = new SBareExpressionContext(_localctx);
				enterOuterAlt(_localctx, 7);
				{
				setState(313);
				exp(0);
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Var_declContext extends ParserRuleContext {
		public Var_nameContext var_name() {
			return getRuleContext(Var_nameContext.class,0);
		}
		public TerminalNode COLON() { return getToken(FifthParser.COLON, 0); }
		public Type_nameContext type_name() {
			return getRuleContext(Type_nameContext.class,0);
		}
		public List_type_signatureContext list_type_signature() {
			return getRuleContext(List_type_signatureContext.class,0);
		}
		public Var_declContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_var_decl; }
	}

	public final Var_declContext var_decl() throws RecognitionException {
		Var_declContext _localctx = new Var_declContext(_ctx, getState());
		enterRule(_localctx, 60, RULE_var_decl);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(316);
			var_name();
			setState(317);
			match(COLON);
			setState(320);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,23,_ctx) ) {
			case 1:
				{
				setState(318);
				type_name();
				}
				break;
			case 2:
				{
				setState(319);
				list_type_signature();
				}
				break;
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ExplistContext extends ParserRuleContext {
		public List<ExpContext> exp() {
			return getRuleContexts(ExpContext.class);
		}
		public ExpContext exp(int i) {
			return getRuleContext(ExpContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(FifthParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(FifthParser.COMMA, i);
		}
		public ExplistContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_explist; }
	}

	public final ExplistContext explist() throws RecognitionException {
		ExplistContext _localctx = new ExplistContext(_ctx, getState());
		enterRule(_localctx, 62, RULE_explist);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(322);
			exp(0);
			setState(327);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(323);
				match(COMMA);
				setState(324);
				exp(0);
				}
				}
				setState(329);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ExpContext extends ParserRuleContext {
		public ExpContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_exp; }
	 
		public ExpContext() { }
		public void copyFrom(ExpContext ctx) {
			super.copyFrom(ctx);
		}
	}
	public static class EFuncCallContext extends ExpContext {
		public Function_nameContext funcname;
		public ExplistContext args;
		public TerminalNode OPENPAREN() { return getToken(FifthParser.OPENPAREN, 0); }
		public TerminalNode CLOSEPAREN() { return getToken(FifthParser.CLOSEPAREN, 0); }
		public Function_nameContext function_name() {
			return getRuleContext(Function_nameContext.class,0);
		}
		public ExplistContext explist() {
			return getRuleContext(ExplistContext.class,0);
		}
		public EFuncCallContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EBoolContext extends ExpContext {
		public Truth_valueContext value;
		public Truth_valueContext truth_value() {
			return getRuleContext(Truth_valueContext.class,0);
		}
		public EBoolContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EVarnameContext extends ExpContext {
		public Var_nameContext var_name() {
			return getRuleContext(Var_nameContext.class,0);
		}
		public EVarnameContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EBooleanContext extends ExpContext {
		public Truth_valueContext value;
		public Truth_valueContext truth_value() {
			return getRuleContext(Truth_valueContext.class,0);
		}
		public EBooleanContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EArithNegationContext extends ExpContext {
		public ExpContext operand;
		public TerminalNode MINUS() { return getToken(FifthParser.MINUS, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public EArithNegationContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class ETypeCastContext extends ExpContext {
		public Type_nameContext type;
		public ExpContext subexp;
		public TerminalNode OPENPAREN() { return getToken(FifthParser.OPENPAREN, 0); }
		public TerminalNode CLOSEPAREN() { return getToken(FifthParser.CLOSEPAREN, 0); }
		public Type_nameContext type_name() {
			return getRuleContext(Type_nameContext.class,0);
		}
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public ETypeCastContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EIntContext extends ExpContext {
		public Token value;
		public TerminalNode INT() { return getToken(FifthParser.INT, 0); }
		public EIntContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class ELTContext extends ExpContext {
		public ExpContext left;
		public ExpContext right;
		public TerminalNode LT() { return getToken(FifthParser.LT, 0); }
		public List<ExpContext> exp() {
			return getRuleContexts(ExpContext.class);
		}
		public ExpContext exp(int i) {
			return getRuleContext(ExpContext.class,i);
		}
		public ELTContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EDivContext extends ExpContext {
		public ExpContext left;
		public ExpContext right;
		public TerminalNode DIVIDE() { return getToken(FifthParser.DIVIDE, 0); }
		public List<ExpContext> exp() {
			return getRuleContexts(ExpContext.class);
		}
		public ExpContext exp(int i) {
			return getRuleContext(ExpContext.class,i);
		}
		public EDivContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EGEQContext extends ExpContext {
		public ExpContext left;
		public ExpContext right;
		public TerminalNode GEQ() { return getToken(FifthParser.GEQ, 0); }
		public List<ExpContext> exp() {
			return getRuleContexts(ExpContext.class);
		}
		public ExpContext exp(int i) {
			return getRuleContext(ExpContext.class,i);
		}
		public EGEQContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class ELogicNegationContext extends ExpContext {
		public ExpContext operand;
		public TerminalNode NOT() { return getToken(FifthParser.NOT, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public ELogicNegationContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EAndContext extends ExpContext {
		public ExpContext left;
		public ExpContext right;
		public TerminalNode AND() { return getToken(FifthParser.AND, 0); }
		public List<ExpContext> exp() {
			return getRuleContexts(ExpContext.class);
		}
		public ExpContext exp(int i) {
			return getRuleContext(ExpContext.class,i);
		}
		public EAndContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EListContext extends ExpContext {
		public ListContext value;
		public ListContext list() {
			return getRuleContext(ListContext.class,0);
		}
		public EListContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EGTContext extends ExpContext {
		public ExpContext left;
		public ExpContext right;
		public TerminalNode GT() { return getToken(FifthParser.GT, 0); }
		public List<ExpContext> exp() {
			return getRuleContexts(ExpContext.class);
		}
		public ExpContext exp(int i) {
			return getRuleContext(ExpContext.class,i);
		}
		public EGTContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class ELEQContext extends ExpContext {
		public ExpContext left;
		public ExpContext right;
		public TerminalNode LEQ() { return getToken(FifthParser.LEQ, 0); }
		public List<ExpContext> exp() {
			return getRuleContexts(ExpContext.class);
		}
		public ExpContext exp(int i) {
			return getRuleContext(ExpContext.class,i);
		}
		public ELEQContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class ETypeCreateInstContext extends ExpContext {
		public TerminalNode NEW() { return getToken(FifthParser.NEW, 0); }
		public Type_initialiserContext type_initialiser() {
			return getRuleContext(Type_initialiserContext.class,0);
		}
		public ETypeCreateInstContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EParenContext extends ExpContext {
		public ExpContext innerexp;
		public TerminalNode OPENPAREN() { return getToken(FifthParser.OPENPAREN, 0); }
		public TerminalNode CLOSEPAREN() { return getToken(FifthParser.CLOSEPAREN, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public EParenContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class ESubContext extends ExpContext {
		public ExpContext left;
		public ExpContext right;
		public TerminalNode MINUS() { return getToken(FifthParser.MINUS, 0); }
		public List<ExpContext> exp() {
			return getRuleContexts(ExpContext.class);
		}
		public ExpContext exp(int i) {
			return getRuleContext(ExpContext.class,i);
		}
		public ESubContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EDoubleContext extends ExpContext {
		public Token value;
		public TerminalNode FLOAT() { return getToken(FifthParser.FLOAT, 0); }
		public EDoubleContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EAddContext extends ExpContext {
		public ExpContext left;
		public ExpContext right;
		public TerminalNode PLUS() { return getToken(FifthParser.PLUS, 0); }
		public List<ExpContext> exp() {
			return getRuleContexts(ExpContext.class);
		}
		public ExpContext exp(int i) {
			return getRuleContext(ExpContext.class,i);
		}
		public EAddContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EStringContext extends ExpContext {
		public Token value;
		public TerminalNode STRING() { return getToken(FifthParser.STRING, 0); }
		public EStringContext(ExpContext ctx) { copyFrom(ctx); }
	}
	public static class EMulContext extends ExpContext {
		public ExpContext left;
		public ExpContext right;
		public TerminalNode TIMES() { return getToken(FifthParser.TIMES, 0); }
		public List<ExpContext> exp() {
			return getRuleContexts(ExpContext.class);
		}
		public ExpContext exp(int i) {
			return getRuleContext(ExpContext.class,i);
		}
		public EMulContext(ExpContext ctx) { copyFrom(ctx); }
	}

	public final ExpContext exp() throws RecognitionException {
		return exp(0);
	}

	private ExpContext exp(int _p) throws RecognitionException {
		ParserRuleContext _parentctx = _ctx;
		int _parentState = getState();
		ExpContext _localctx = new ExpContext(_ctx, _parentState);
		ExpContext _prevctx = _localctx;
		int _startState = 64;
		enterRecursionRule(_localctx, 64, RULE_exp, _p);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(360);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,26,_ctx) ) {
			case 1:
				{
				_localctx = new ETypeCastContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;

				setState(331);
				match(OPENPAREN);
				setState(332);
				((ETypeCastContext)_localctx).type = type_name();
				setState(333);
				match(CLOSEPAREN);
				setState(334);
				((ETypeCastContext)_localctx).subexp = exp(22);
				}
				break;
			case 2:
				{
				_localctx = new EBoolContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(336);
				((EBoolContext)_localctx).value = truth_value();
				}
				break;
			case 3:
				{
				_localctx = new EIntContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(337);
				((EIntContext)_localctx).value = match(INT);
				}
				break;
			case 4:
				{
				_localctx = new EDoubleContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(338);
				((EDoubleContext)_localctx).value = match(FLOAT);
				}
				break;
			case 5:
				{
				_localctx = new EStringContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(339);
				((EStringContext)_localctx).value = match(STRING);
				}
				break;
			case 6:
				{
				_localctx = new EBooleanContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(340);
				((EBooleanContext)_localctx).value = truth_value();
				}
				break;
			case 7:
				{
				_localctx = new EListContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(341);
				((EListContext)_localctx).value = list();
				}
				break;
			case 8:
				{
				_localctx = new ELogicNegationContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(342);
				match(NOT);
				setState(343);
				((ELogicNegationContext)_localctx).operand = exp(15);
				}
				break;
			case 9:
				{
				_localctx = new EArithNegationContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(344);
				match(MINUS);
				setState(345);
				((EArithNegationContext)_localctx).operand = exp(14);
				}
				break;
			case 10:
				{
				_localctx = new EVarnameContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(346);
				var_name();
				}
				break;
			case 11:
				{
				_localctx = new EFuncCallContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(347);
				((EFuncCallContext)_localctx).funcname = function_name();
				setState(348);
				match(OPENPAREN);
				setState(350);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << FALSE) | (1L << NEW) | (1L << TRUE) | (1L << MINUS) | (1L << NOT) | (1L << OPENBRACK) | (1L << OPENPAREN) | (1L << IDENTIFIER) | (1L << STRING) | (1L << INT) | (1L << FLOAT))) != 0)) {
					{
					setState(349);
					((EFuncCallContext)_localctx).args = explist();
					}
				}

				setState(352);
				match(CLOSEPAREN);
				}
				break;
			case 12:
				{
				_localctx = new EParenContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(354);
				match(OPENPAREN);
				setState(355);
				((EParenContext)_localctx).innerexp = exp(0);
				setState(356);
				match(CLOSEPAREN);
				}
				break;
			case 13:
				{
				_localctx = new ETypeCreateInstContext(_localctx);
				_ctx = _localctx;
				_prevctx = _localctx;
				setState(358);
				match(NEW);
				setState(359);
				type_initialiser();
				}
				break;
			}
			_ctx.stop = _input.LT(-1);
			setState(391);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,28,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					setState(389);
					_errHandler.sync(this);
					switch ( getInterpreter().adaptivePredict(_input,27,_ctx) ) {
					case 1:
						{
						_localctx = new ELTContext(new ExpContext(_parentctx, _parentState));
						((ELTContext)_localctx).left = _prevctx;
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(362);
						if (!(precpred(_ctx, 13))) throw new FailedPredicateException(this, "precpred(_ctx, 13)");
						setState(363);
						match(LT);
						setState(364);
						((ELTContext)_localctx).right = exp(14);
						}
						break;
					case 2:
						{
						_localctx = new EGTContext(new ExpContext(_parentctx, _parentState));
						((EGTContext)_localctx).left = _prevctx;
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(365);
						if (!(precpred(_ctx, 12))) throw new FailedPredicateException(this, "precpred(_ctx, 12)");
						setState(366);
						match(GT);
						setState(367);
						((EGTContext)_localctx).right = exp(13);
						}
						break;
					case 3:
						{
						_localctx = new ELEQContext(new ExpContext(_parentctx, _parentState));
						((ELEQContext)_localctx).left = _prevctx;
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(368);
						if (!(precpred(_ctx, 11))) throw new FailedPredicateException(this, "precpred(_ctx, 11)");
						setState(369);
						match(LEQ);
						setState(370);
						((ELEQContext)_localctx).right = exp(12);
						}
						break;
					case 4:
						{
						_localctx = new EGEQContext(new ExpContext(_parentctx, _parentState));
						((EGEQContext)_localctx).left = _prevctx;
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(371);
						if (!(precpred(_ctx, 10))) throw new FailedPredicateException(this, "precpred(_ctx, 10)");
						setState(372);
						match(GEQ);
						setState(373);
						((EGEQContext)_localctx).right = exp(11);
						}
						break;
					case 5:
						{
						_localctx = new EAndContext(new ExpContext(_parentctx, _parentState));
						((EAndContext)_localctx).left = _prevctx;
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(374);
						if (!(precpred(_ctx, 9))) throw new FailedPredicateException(this, "precpred(_ctx, 9)");
						setState(375);
						match(AND);
						setState(376);
						((EAndContext)_localctx).right = exp(10);
						}
						break;
					case 6:
						{
						_localctx = new EAddContext(new ExpContext(_parentctx, _parentState));
						((EAddContext)_localctx).left = _prevctx;
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(377);
						if (!(precpred(_ctx, 8))) throw new FailedPredicateException(this, "precpred(_ctx, 8)");
						setState(378);
						match(PLUS);
						setState(379);
						((EAddContext)_localctx).right = exp(9);
						}
						break;
					case 7:
						{
						_localctx = new ESubContext(new ExpContext(_parentctx, _parentState));
						((ESubContext)_localctx).left = _prevctx;
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(380);
						if (!(precpred(_ctx, 7))) throw new FailedPredicateException(this, "precpred(_ctx, 7)");
						setState(381);
						match(MINUS);
						setState(382);
						((ESubContext)_localctx).right = exp(8);
						}
						break;
					case 8:
						{
						_localctx = new EMulContext(new ExpContext(_parentctx, _parentState));
						((EMulContext)_localctx).left = _prevctx;
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(383);
						if (!(precpred(_ctx, 6))) throw new FailedPredicateException(this, "precpred(_ctx, 6)");
						setState(384);
						match(TIMES);
						setState(385);
						((EMulContext)_localctx).right = exp(7);
						}
						break;
					case 9:
						{
						_localctx = new EDivContext(new ExpContext(_parentctx, _parentState));
						((EDivContext)_localctx).left = _prevctx;
						pushNewRecursionContext(_localctx, _startState, RULE_exp);
						setState(386);
						if (!(precpred(_ctx, 5))) throw new FailedPredicateException(this, "precpred(_ctx, 5)");
						setState(387);
						match(DIVIDE);
						setState(388);
						((EDivContext)_localctx).right = exp(6);
						}
						break;
					}
					} 
				}
				setState(393);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,28,_ctx);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			unrollRecursionContexts(_parentctx);
		}
		return _localctx;
	}

	public static class Truth_valueContext extends ParserRuleContext {
		public Token value;
		public TerminalNode TRUE() { return getToken(FifthParser.TRUE, 0); }
		public TerminalNode FALSE() { return getToken(FifthParser.FALSE, 0); }
		public Truth_valueContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_truth_value; }
	}

	public final Truth_valueContext truth_value() throws RecognitionException {
		Truth_valueContext _localctx = new Truth_valueContext(_ctx, getState());
		enterRule(_localctx, 66, RULE_truth_value);
		try {
			setState(396);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case TRUE:
				enterOuterAlt(_localctx, 1);
				{
				setState(394);
				((Truth_valueContext)_localctx).value = match(TRUE);
				}
				break;
			case FALSE:
				enterOuterAlt(_localctx, 2);
				{
				setState(395);
				((Truth_valueContext)_localctx).value = match(FALSE);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Identifier_chainContext extends ParserRuleContext {
		public Token IDENTIFIER;
		public List<Token> segments = new ArrayList<Token>();
		public List<TerminalNode> IDENTIFIER() { return getTokens(FifthParser.IDENTIFIER); }
		public TerminalNode IDENTIFIER(int i) {
			return getToken(FifthParser.IDENTIFIER, i);
		}
		public List<TerminalNode> DOT() { return getTokens(FifthParser.DOT); }
		public TerminalNode DOT(int i) {
			return getToken(FifthParser.DOT, i);
		}
		public Identifier_chainContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_identifier_chain; }
	}

	public final Identifier_chainContext identifier_chain() throws RecognitionException {
		Identifier_chainContext _localctx = new Identifier_chainContext(_ctx, getState());
		enterRule(_localctx, 68, RULE_identifier_chain);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(398);
			((Identifier_chainContext)_localctx).IDENTIFIER = match(IDENTIFIER);
			((Identifier_chainContext)_localctx).segments.add(((Identifier_chainContext)_localctx).IDENTIFIER);
			setState(403);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,30,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(399);
					match(DOT);
					setState(400);
					((Identifier_chainContext)_localctx).IDENTIFIER = match(IDENTIFIER);
					((Identifier_chainContext)_localctx).segments.add(((Identifier_chainContext)_localctx).IDENTIFIER);
					}
					} 
				}
				setState(405);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,30,_ctx);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Var_nameContext extends ParserRuleContext {
		public Identifier_chainContext identifier_chain() {
			return getRuleContext(Identifier_chainContext.class,0);
		}
		public Var_nameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_var_name; }
	}

	public final Var_nameContext var_name() throws RecognitionException {
		Var_nameContext _localctx = new Var_nameContext(_ctx, getState());
		enterRule(_localctx, 70, RULE_var_name);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(406);
			identifier_chain();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class AliasContext extends ParserRuleContext {
		public PackagenameContext name;
		public AbsoluteIriContext uri;
		public TerminalNode ALIAS() { return getToken(FifthParser.ALIAS, 0); }
		public TerminalNode AS() { return getToken(FifthParser.AS, 0); }
		public TerminalNode SEMICOLON() { return getToken(FifthParser.SEMICOLON, 0); }
		public PackagenameContext packagename() {
			return getRuleContext(PackagenameContext.class,0);
		}
		public AbsoluteIriContext absoluteIri() {
			return getRuleContext(AbsoluteIriContext.class,0);
		}
		public AliasContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_alias; }
	}

	public final AliasContext alias() throws RecognitionException {
		AliasContext _localctx = new AliasContext(_ctx, getState());
		enterRule(_localctx, 72, RULE_alias);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(408);
			match(ALIAS);
			setState(409);
			((AliasContext)_localctx).name = packagename();
			setState(410);
			match(AS);
			setState(411);
			((AliasContext)_localctx).uri = absoluteIri();
			setState(412);
			match(SEMICOLON);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class IriContext extends ParserRuleContext {
		public QNameIriContext qNameIri() {
			return getRuleContext(QNameIriContext.class,0);
		}
		public AbsoluteIriContext absoluteIri() {
			return getRuleContext(AbsoluteIriContext.class,0);
		}
		public IriContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_iri; }
	}

	public final IriContext iri() throws RecognitionException {
		IriContext _localctx = new IriContext(_ctx, getState());
		enterRule(_localctx, 74, RULE_iri);
		try {
			setState(416);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,31,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(414);
				qNameIri();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(415);
				absoluteIri();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class QNameIriContext extends ParserRuleContext {
		public Token prefix;
		public Token fragname;
		public TerminalNode COLON() { return getToken(FifthParser.COLON, 0); }
		public List<TerminalNode> IDENTIFIER() { return getTokens(FifthParser.IDENTIFIER); }
		public TerminalNode IDENTIFIER(int i) {
			return getToken(FifthParser.IDENTIFIER, i);
		}
		public QNameIriContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_qNameIri; }
	}

	public final QNameIriContext qNameIri() throws RecognitionException {
		QNameIriContext _localctx = new QNameIriContext(_ctx, getState());
		enterRule(_localctx, 76, RULE_qNameIri);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(419);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==IDENTIFIER) {
				{
				setState(418);
				((QNameIriContext)_localctx).prefix = match(IDENTIFIER);
				}
			}

			setState(421);
			match(COLON);
			setState(422);
			((QNameIriContext)_localctx).fragname = match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class AbsoluteIriContext extends ParserRuleContext {
		public Token iri_scheme;
		public Token IDENTIFIER;
		public List<Token> iri_domain = new ArrayList<Token>();
		public List<Token> iri_segment = new ArrayList<Token>();
		public TerminalNode COLON() { return getToken(FifthParser.COLON, 0); }
		public List<TerminalNode> DIVIDE() { return getTokens(FifthParser.DIVIDE); }
		public TerminalNode DIVIDE(int i) {
			return getToken(FifthParser.DIVIDE, i);
		}
		public List<TerminalNode> IDENTIFIER() { return getTokens(FifthParser.IDENTIFIER); }
		public TerminalNode IDENTIFIER(int i) {
			return getToken(FifthParser.IDENTIFIER, i);
		}
		public List<TerminalNode> DOT() { return getTokens(FifthParser.DOT); }
		public TerminalNode DOT(int i) {
			return getToken(FifthParser.DOT, i);
		}
		public TerminalNode HASH() { return getToken(FifthParser.HASH, 0); }
		public AbsoluteIriContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_absoluteIri; }
	}

	public final AbsoluteIriContext absoluteIri() throws RecognitionException {
		AbsoluteIriContext _localctx = new AbsoluteIriContext(_ctx, getState());
		enterRule(_localctx, 78, RULE_absoluteIri);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(424);
			((AbsoluteIriContext)_localctx).iri_scheme = match(IDENTIFIER);
			setState(425);
			match(COLON);
			setState(426);
			match(DIVIDE);
			setState(427);
			match(DIVIDE);
			setState(428);
			((AbsoluteIriContext)_localctx).IDENTIFIER = match(IDENTIFIER);
			((AbsoluteIriContext)_localctx).iri_domain.add(((AbsoluteIriContext)_localctx).IDENTIFIER);
			setState(433);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==DOT) {
				{
				{
				setState(429);
				match(DOT);
				setState(430);
				((AbsoluteIriContext)_localctx).IDENTIFIER = match(IDENTIFIER);
				((AbsoluteIriContext)_localctx).iri_domain.add(((AbsoluteIriContext)_localctx).IDENTIFIER);
				}
				}
				setState(435);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(440);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,34,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(436);
					match(DIVIDE);
					setState(437);
					((AbsoluteIriContext)_localctx).IDENTIFIER = match(IDENTIFIER);
					((AbsoluteIriContext)_localctx).iri_segment.add(((AbsoluteIriContext)_localctx).IDENTIFIER);
					}
					} 
				}
				setState(442);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,34,_ctx);
			}
			setState(444);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==DIVIDE) {
				{
				setState(443);
				match(DIVIDE);
				}
			}

			setState(450);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==HASH) {
				{
				setState(446);
				match(HASH);
				setState(448);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==IDENTIFIER) {
					{
					setState(447);
					match(IDENTIFIER);
					}
				}

				}
			}

			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Iri_query_paramContext extends ParserRuleContext {
		public Token name;
		public Token val;
		public TerminalNode ASSIGN() { return getToken(FifthParser.ASSIGN, 0); }
		public List<TerminalNode> IDENTIFIER() { return getTokens(FifthParser.IDENTIFIER); }
		public TerminalNode IDENTIFIER(int i) {
			return getToken(FifthParser.IDENTIFIER, i);
		}
		public Iri_query_paramContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_iri_query_param; }
	}

	public final Iri_query_paramContext iri_query_param() throws RecognitionException {
		Iri_query_paramContext _localctx = new Iri_query_paramContext(_ctx, getState());
		enterRule(_localctx, 80, RULE_iri_query_param);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(452);
			((Iri_query_paramContext)_localctx).name = match(IDENTIFIER);
			setState(453);
			match(ASSIGN);
			setState(454);
			((Iri_query_paramContext)_localctx).val = match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class List_type_signatureContext extends ParserRuleContext {
		public Type_nameContext type_name() {
			return getRuleContext(Type_nameContext.class,0);
		}
		public TerminalNode OPENBRACK() { return getToken(FifthParser.OPENBRACK, 0); }
		public TerminalNode CLOSEBRACK() { return getToken(FifthParser.CLOSEBRACK, 0); }
		public List_type_signatureContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_list_type_signature; }
	}

	public final List_type_signatureContext list_type_signature() throws RecognitionException {
		List_type_signatureContext _localctx = new List_type_signatureContext(_ctx, getState());
		enterRule(_localctx, 82, RULE_list_type_signature);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(456);
			type_name();
			setState(457);
			match(OPENBRACK);
			setState(458);
			match(CLOSEBRACK);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ListContext extends ParserRuleContext {
		public List_bodyContext body;
		public TerminalNode OPENBRACK() { return getToken(FifthParser.OPENBRACK, 0); }
		public TerminalNode CLOSEBRACK() { return getToken(FifthParser.CLOSEBRACK, 0); }
		public List_bodyContext list_body() {
			return getRuleContext(List_bodyContext.class,0);
		}
		public ListContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_list; }
	}

	public final ListContext list() throws RecognitionException {
		ListContext _localctx = new ListContext(_ctx, getState());
		enterRule(_localctx, 84, RULE_list);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(460);
			match(OPENBRACK);
			setState(461);
			((ListContext)_localctx).body = list_body();
			setState(462);
			match(CLOSEBRACK);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class List_bodyContext extends ParserRuleContext {
		public List_bodyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_list_body; }
	 
		public List_bodyContext() { }
		public void copyFrom(List_bodyContext ctx) {
			super.copyFrom(ctx);
		}
	}
	public static class EListLiteralContext extends List_bodyContext {
		public List_literalContext list_literal() {
			return getRuleContext(List_literalContext.class,0);
		}
		public EListLiteralContext(List_bodyContext ctx) { copyFrom(ctx); }
	}
	public static class EListComprehensionContext extends List_bodyContext {
		public List_comprehensionContext list_comprehension() {
			return getRuleContext(List_comprehensionContext.class,0);
		}
		public EListComprehensionContext(List_bodyContext ctx) { copyFrom(ctx); }
	}

	public final List_bodyContext list_body() throws RecognitionException {
		List_bodyContext _localctx = new List_bodyContext(_ctx, getState());
		enterRule(_localctx, 86, RULE_list_body);
		try {
			setState(466);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,38,_ctx) ) {
			case 1:
				_localctx = new EListLiteralContext(_localctx);
				enterOuterAlt(_localctx, 1);
				{
				setState(464);
				list_literal();
				}
				break;
			case 2:
				_localctx = new EListComprehensionContext(_localctx);
				enterOuterAlt(_localctx, 2);
				{
				setState(465);
				list_comprehension();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class List_literalContext extends ParserRuleContext {
		public ExplistContext explist() {
			return getRuleContext(ExplistContext.class,0);
		}
		public List_literalContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_list_literal; }
	}

	public final List_literalContext list_literal() throws RecognitionException {
		List_literalContext _localctx = new List_literalContext(_ctx, getState());
		enterRule(_localctx, 88, RULE_list_literal);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(468);
			explist();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class List_comprehensionContext extends ParserRuleContext {
		public Var_nameContext varname;
		public List_comp_generatorContext gen;
		public List_comp_constraintContext constraints;
		public TerminalNode BAR() { return getToken(FifthParser.BAR, 0); }
		public Var_nameContext var_name() {
			return getRuleContext(Var_nameContext.class,0);
		}
		public List_comp_generatorContext list_comp_generator() {
			return getRuleContext(List_comp_generatorContext.class,0);
		}
		public TerminalNode COMMA() { return getToken(FifthParser.COMMA, 0); }
		public List_comp_constraintContext list_comp_constraint() {
			return getRuleContext(List_comp_constraintContext.class,0);
		}
		public List_comprehensionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_list_comprehension; }
	}

	public final List_comprehensionContext list_comprehension() throws RecognitionException {
		List_comprehensionContext _localctx = new List_comprehensionContext(_ctx, getState());
		enterRule(_localctx, 90, RULE_list_comprehension);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(470);
			((List_comprehensionContext)_localctx).varname = var_name();
			setState(471);
			match(BAR);
			setState(472);
			((List_comprehensionContext)_localctx).gen = list_comp_generator();
			{
			setState(473);
			match(COMMA);
			setState(474);
			((List_comprehensionContext)_localctx).constraints = list_comp_constraint();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class List_comp_generatorContext extends ParserRuleContext {
		public Var_nameContext varname;
		public Var_nameContext value;
		public TerminalNode GEN() { return getToken(FifthParser.GEN, 0); }
		public List<Var_nameContext> var_name() {
			return getRuleContexts(Var_nameContext.class);
		}
		public Var_nameContext var_name(int i) {
			return getRuleContext(Var_nameContext.class,i);
		}
		public List_comp_generatorContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_list_comp_generator; }
	}

	public final List_comp_generatorContext list_comp_generator() throws RecognitionException {
		List_comp_generatorContext _localctx = new List_comp_generatorContext(_ctx, getState());
		enterRule(_localctx, 92, RULE_list_comp_generator);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(476);
			((List_comp_generatorContext)_localctx).varname = var_name();
			setState(477);
			match(GEN);
			setState(478);
			((List_comp_generatorContext)_localctx).value = var_name();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class List_comp_constraintContext extends ParserRuleContext {
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public List_comp_constraintContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_list_comp_constraint; }
	}

	public final List_comp_constraintContext list_comp_constraint() throws RecognitionException {
		List_comp_constraintContext _localctx = new List_comp_constraintContext(_ctx, getState());
		enterRule(_localctx, 94, RULE_list_comp_constraint);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(480);
			exp(0);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public boolean sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 32:
			return exp_sempred((ExpContext)_localctx, predIndex);
		}
		return true;
	}
	private boolean exp_sempred(ExpContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0:
			return precpred(_ctx, 13);
		case 1:
			return precpred(_ctx, 12);
		case 2:
			return precpred(_ctx, 11);
		case 3:
			return precpred(_ctx, 10);
		case 4:
			return precpred(_ctx, 9);
		case 5:
			return precpred(_ctx, 8);
		case 6:
			return precpred(_ctx, 7);
		case 7:
			return precpred(_ctx, 6);
		case 8:
			return precpred(_ctx, 5);
		}
		return true;
	}

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\3\65\u01e5\4\2\t\2"+
		"\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13"+
		"\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22\t\22"+
		"\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31\t\31"+
		"\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t \4!"+
		"\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\4)\t)\4*\t*\4+\t+\4"+
		",\t,\4-\t-\4.\t.\4/\t/\4\60\t\60\4\61\t\61\3\2\7\2d\n\2\f\2\16\2g\13\2"+
		"\3\2\7\2j\n\2\f\2\16\2m\13\2\3\2\3\2\7\2q\n\2\f\2\16\2t\13\2\3\3\3\3\3"+
		"\3\3\3\3\3\7\3{\n\3\f\3\16\3~\13\3\3\3\3\3\3\4\3\4\3\4\3\4\7\4\u0086\n"+
		"\4\f\4\16\4\u0089\13\4\3\4\3\4\3\5\3\5\3\6\3\6\3\7\3\7\3\7\3\7\3\7\7\7"+
		"\u0096\n\7\f\7\16\7\u0099\13\7\3\7\3\7\3\b\3\b\3\b\3\b\3\b\3\t\3\t\3\t"+
		"\3\n\3\n\3\n\3\n\3\n\7\n\u00aa\n\n\f\n\16\n\u00ad\13\n\3\n\3\n\3\13\3"+
		"\13\3\f\3\f\3\f\3\f\3\r\3\r\3\r\3\r\3\r\3\r\3\16\3\16\3\17\3\17\3\20\3"+
		"\20\3\21\3\21\3\21\7\21\u00c6\n\21\f\21\16\21\u00c9\13\21\3\22\3\22\5"+
		"\22\u00cd\n\22\3\22\3\22\3\23\3\23\3\23\3\23\5\23\u00d5\n\23\3\23\5\23"+
		"\u00d8\n\23\3\24\3\24\3\24\3\24\3\24\3\24\3\24\7\24\u00e1\n\24\f\24\16"+
		"\24\u00e4\13\24\3\24\3\24\3\25\3\25\3\25\3\25\5\25\u00ec\n\25\3\26\3\26"+
		"\3\26\3\27\3\27\3\30\3\30\3\31\3\31\3\31\3\31\5\31\u00f9\n\31\3\31\5\31"+
		"\u00fc\n\31\3\32\3\32\3\33\3\33\3\34\3\34\3\34\3\34\7\34\u0106\n\34\f"+
		"\34\16\34\u0109\13\34\3\34\3\34\3\35\3\35\3\35\3\35\5\35\u0111\n\35\3"+
		"\36\3\36\3\36\3\36\7\36\u0117\n\36\f\36\16\36\u011a\13\36\3\36\3\36\3"+
		"\37\3\37\3\37\3\37\3\37\3\37\3\37\5\37\u0125\n\37\3\37\3\37\3\37\3\37"+
		"\3\37\3\37\3\37\3\37\3\37\3\37\3\37\3\37\3\37\5\37\u0134\n\37\3\37\3\37"+
		"\3\37\3\37\3\37\3\37\3\37\5\37\u013d\n\37\3 \3 \3 \3 \5 \u0143\n \3!\3"+
		"!\3!\7!\u0148\n!\f!\16!\u014b\13!\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\""+
		"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\5\"\u0161\n\"\3\"\3\"\3\""+
		"\3\"\3\"\3\"\3\"\3\"\5\"\u016b\n\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\""+
		"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3\"\3"+
		"\"\7\"\u0188\n\"\f\"\16\"\u018b\13\"\3#\3#\5#\u018f\n#\3$\3$\3$\7$\u0194"+
		"\n$\f$\16$\u0197\13$\3%\3%\3&\3&\3&\3&\3&\3&\3\'\3\'\5\'\u01a3\n\'\3("+
		"\5(\u01a6\n(\3(\3(\3(\3)\3)\3)\3)\3)\3)\3)\7)\u01b2\n)\f)\16)\u01b5\13"+
		")\3)\3)\7)\u01b9\n)\f)\16)\u01bc\13)\3)\5)\u01bf\n)\3)\3)\5)\u01c3\n)"+
		"\5)\u01c5\n)\3*\3*\3*\3*\3+\3+\3+\3+\3,\3,\3,\3,\3-\3-\5-\u01d5\n-\3."+
		"\3.\3/\3/\3/\3/\3/\3/\3\60\3\60\3\60\3\60\3\61\3\61\3\61\2\3B\62\2\4\6"+
		"\b\n\f\16\20\22\24\26\30\32\34\36 \"$&(*,.\60\62\64\668:<>@BDFHJLNPRT"+
		"VXZ\\^`\2\2\2\u01f3\2e\3\2\2\2\4u\3\2\2\2\6\u0081\3\2\2\2\b\u008c\3\2"+
		"\2\2\n\u008e\3\2\2\2\f\u0090\3\2\2\2\16\u009c\3\2\2\2\20\u00a1\3\2\2\2"+
		"\22\u00a4\3\2\2\2\24\u00b0\3\2\2\2\26\u00b2\3\2\2\2\30\u00b6\3\2\2\2\32"+
		"\u00bc\3\2\2\2\34\u00be\3\2\2\2\36\u00c0\3\2\2\2 \u00c2\3\2\2\2\"\u00ca"+
		"\3\2\2\2$\u00d7\3\2\2\2&\u00d9\3\2\2\2(\u00e7\3\2\2\2*\u00ed\3\2\2\2,"+
		"\u00f0\3\2\2\2.\u00f2\3\2\2\2\60\u00f4\3\2\2\2\62\u00fd\3\2\2\2\64\u00ff"+
		"\3\2\2\2\66\u0101\3\2\2\28\u010c\3\2\2\2:\u0112\3\2\2\2<\u013c\3\2\2\2"+
		">\u013e\3\2\2\2@\u0144\3\2\2\2B\u016a\3\2\2\2D\u018e\3\2\2\2F\u0190\3"+
		"\2\2\2H\u0198\3\2\2\2J\u019a\3\2\2\2L\u01a2\3\2\2\2N\u01a5\3\2\2\2P\u01aa"+
		"\3\2\2\2R\u01c6\3\2\2\2T\u01ca\3\2\2\2V\u01ce\3\2\2\2X\u01d4\3\2\2\2Z"+
		"\u01d6\3\2\2\2\\\u01d8\3\2\2\2^\u01de\3\2\2\2`\u01e2\3\2\2\2bd\5\6\4\2"+
		"cb\3\2\2\2dg\3\2\2\2ec\3\2\2\2ef\3\2\2\2fk\3\2\2\2ge\3\2\2\2hj\5J&\2i"+
		"h\3\2\2\2jm\3\2\2\2ki\3\2\2\2kl\3\2\2\2lr\3\2\2\2mk\3\2\2\2nq\5\30\r\2"+
		"oq\5\f\7\2pn\3\2\2\2po\3\2\2\2qt\3\2\2\2rp\3\2\2\2rs\3\2\2\2s\3\3\2\2"+
		"\2tr\3\2\2\2uv\5\34\17\2vw\7(\2\2w|\5B\"\2xy\7\30\2\2y{\5B\"\2zx\3\2\2"+
		"\2{~\3\2\2\2|z\3\2\2\2|}\3\2\2\2}\177\3\2\2\2~|\3\2\2\2\177\u0080\7\26"+
		"\2\2\u0080\5\3\2\2\2\u0081\u0082\7\f\2\2\u0082\u0087\5\b\5\2\u0083\u0084"+
		"\7\30\2\2\u0084\u0086\5\b\5\2\u0085\u0083\3\2\2\2\u0086\u0089\3\2\2\2"+
		"\u0087\u0085\3\2\2\2\u0087\u0088\3\2\2\2\u0088\u008a\3\2\2\2\u0089\u0087"+
		"\3\2\2\2\u008a\u008b\7.\2\2\u008b\7\3\2\2\2\u008c\u008d\7\61\2\2\u008d"+
		"\t\3\2\2\2\u008e\u008f\7\61\2\2\u008f\13\3\2\2\2\u0090\u0091\7\5\2\2\u0091"+
		"\u0092\7\61\2\2\u0092\u0097\7&\2\2\u0093\u0096\5\30\r\2\u0094\u0096\5"+
		"\16\b\2\u0095\u0093\3\2\2\2\u0095\u0094\3\2\2\2\u0096\u0099\3\2\2\2\u0097"+
		"\u0095\3\2\2\2\u0097\u0098\3\2\2\2\u0098\u009a\3\2\2\2\u0099\u0097\3\2"+
		"\2\2\u009a\u009b\7\24\2\2\u009b\r\3\2\2\2\u009c\u009d\7\61\2\2\u009d\u009e"+
		"\7\27\2\2\u009e\u009f\7\61\2\2\u009f\u00a0\7.\2\2\u00a0\17\3\2\2\2\u00a1"+
		"\u00a2\7\32\2\2\u00a2\u00a3\7\61\2\2\u00a3\21\3\2\2\2\u00a4\u00a5\5\24"+
		"\13\2\u00a5\u00a6\7&\2\2\u00a6\u00ab\5\26\f\2\u00a7\u00a8\7\30\2\2\u00a8"+
		"\u00aa\5\26\f\2\u00a9\u00a7\3\2\2\2\u00aa\u00ad\3\2\2\2\u00ab\u00a9\3"+
		"\2\2\2\u00ab\u00ac\3\2\2\2\u00ac\u00ae\3\2\2\2\u00ad\u00ab\3\2\2\2\u00ae"+
		"\u00af\7\24\2\2\u00af\23\3\2\2\2\u00b0\u00b1\7\61\2\2\u00b1\25\3\2\2\2"+
		"\u00b2\u00b3\5H%\2\u00b3\u00b4\7\22\2\2\u00b4\u00b5\5B\"\2\u00b5\27\3"+
		"\2\2\2\u00b6\u00b7\5\34\17\2\u00b7\u00b8\5\"\22\2\u00b8\u00b9\7\27\2\2"+
		"\u00b9\u00ba\5\36\20\2\u00ba\u00bb\5\32\16\2\u00bb\31\3\2\2\2\u00bc\u00bd"+
		"\5:\36\2\u00bd\33\3\2\2\2\u00be\u00bf\5F$\2\u00bf\35\3\2\2\2\u00c0\u00c1"+
		"\7\61\2\2\u00c1\37\3\2\2\2\u00c2\u00c7\5\60\31\2\u00c3\u00c4\7\30\2\2"+
		"\u00c4\u00c6\5\60\31\2\u00c5\u00c3\3\2\2\2\u00c6\u00c9\3\2\2\2\u00c7\u00c5"+
		"\3\2\2\2\u00c7\u00c8\3\2\2\2\u00c8!\3\2\2\2\u00c9\u00c7\3\2\2\2\u00ca"+
		"\u00cc\7(\2\2\u00cb\u00cd\5 \21\2\u00cc\u00cb\3\2\2\2\u00cc\u00cd\3\2"+
		"\2\2\u00cd\u00ce\3\2\2\2\u00ce\u00cf\7\26\2\2\u00cf#\3\2\2\2\u00d0\u00d1"+
		"\5.\30\2\u00d1\u00d2\7\27\2\2\u00d2\u00d4\5,\27\2\u00d3\u00d5\5*\26\2"+
		"\u00d4\u00d3\3\2\2\2\u00d4\u00d5\3\2\2\2\u00d5\u00d8\3\2\2\2\u00d6\u00d8"+
		"\5&\24\2\u00d7\u00d0\3\2\2\2\u00d7\u00d6\3\2\2\2\u00d8%\3\2\2\2\u00d9"+
		"\u00da\5.\30\2\u00da\u00db\7\27\2\2\u00db\u00dc\5,\27\2\u00dc\u00dd\7"+
		"&\2\2\u00dd\u00e2\5(\25\2\u00de\u00df\7\30\2\2\u00df\u00e1\5(\25\2\u00e0"+
		"\u00de\3\2\2\2\u00e1\u00e4\3\2\2\2\u00e2\u00e0\3\2\2\2\u00e2\u00e3\3\2"+
		"\2\2\u00e3\u00e5\3\2\2\2\u00e4\u00e2\3\2\2\2\u00e5\u00e6\7\24\2\2\u00e6"+
		"\'\3\2\2\2\u00e7\u00e8\5H%\2\u00e8\u00e9\7\27\2\2\u00e9\u00eb\5H%\2\u00ea"+
		"\u00ec\5*\26\2\u00eb\u00ea\3\2\2\2\u00eb\u00ec\3\2\2\2\u00ec)\3\2\2\2"+
		"\u00ed\u00ee\7\23\2\2\u00ee\u00ef\5B\"\2\u00ef+\3\2\2\2\u00f0\u00f1\5"+
		"F$\2\u00f1-\3\2\2\2\u00f2\u00f3\7\61\2\2\u00f3/\3\2\2\2\u00f4\u00f5\5"+
		"\62\32\2\u00f5\u00f6\7\27\2\2\u00f6\u00fb\5\64\33\2\u00f7\u00f9\5*\26"+
		"\2\u00f8\u00f7\3\2\2\2\u00f8\u00f9\3\2\2\2\u00f9\u00fc\3\2\2\2\u00fa\u00fc"+
		"\5\66\34\2\u00fb\u00f8\3\2\2\2\u00fb\u00fa\3\2\2\2\u00fb\u00fc\3\2\2\2"+
		"\u00fc\61\3\2\2\2\u00fd\u00fe\7\61\2\2\u00fe\63\3\2\2\2\u00ff\u0100\5"+
		"F$\2\u0100\65\3\2\2\2\u0101\u0102\7&\2\2\u0102\u0107\58\35\2\u0103\u0104"+
		"\7\30\2\2\u0104\u0106\58\35\2\u0105\u0103\3\2\2\2\u0106\u0109\3\2\2\2"+
		"\u0107\u0105\3\2\2\2\u0107\u0108\3\2\2\2\u0108\u010a\3\2\2\2\u0109\u0107"+
		"\3\2\2\2\u010a\u010b\7\24\2\2\u010b\67\3\2\2\2\u010c\u010d\5\62\32\2\u010d"+
		"\u010e\7\27\2\2\u010e\u0110\5\62\32\2\u010f\u0111\5\66\34\2\u0110\u010f"+
		"\3\2\2\2\u0110\u0111\3\2\2\2\u01119\3\2\2\2\u0112\u0118\7&\2\2\u0113\u0114"+
		"\5<\37\2\u0114\u0115\7.\2\2\u0115\u0117\3\2\2\2\u0116\u0113\3\2\2\2\u0117"+
		"\u011a\3\2\2\2\u0118\u0116\3\2\2\2\u0118\u0119\3\2\2\2\u0119\u011b\3\2"+
		"\2\2\u011a\u0118\3\2\2\2\u011b\u011c\7\24\2\2\u011c;\3\2\2\2\u011d\u011e"+
		"\7\b\2\2\u011e\u011f\7(\2\2\u011f\u0120\5B\"\2\u0120\u0121\7\26\2\2\u0121"+
		"\u0124\5:\36\2\u0122\u0123\7\6\2\2\u0123\u0125\5:\36\2\u0124\u0122\3\2"+
		"\2\2\u0124\u0125\3\2\2\2\u0125\u013d\3\2\2\2\u0126\u0127\7\16\2\2\u0127"+
		"\u0128\7(\2\2\u0128\u0129\5B\"\2\u0129\u012a\7\26\2\2\u012a\u012b\5:\36"+
		"\2\u012b\u013d\3\2\2\2\u012c\u012d\7\17\2\2\u012d\u012e\5B\"\2\u012e\u012f"+
		"\5:\36\2\u012f\u013d\3\2\2\2\u0130\u0133\5> \2\u0131\u0132\7\22\2\2\u0132"+
		"\u0134\5B\"\2\u0133\u0131\3\2\2\2\u0133\u0134\3\2\2\2\u0134\u013d\3\2"+
		"\2\2\u0135\u0136\5H%\2\u0136\u0137\7\22\2\2\u0137\u0138\5B\"\2\u0138\u013d"+
		"\3\2\2\2\u0139\u013a\7\13\2\2\u013a\u013d\5B\"\2\u013b\u013d\5B\"\2\u013c"+
		"\u011d\3\2\2\2\u013c\u0126\3\2\2\2\u013c\u012c\3\2\2\2\u013c\u0130\3\2"+
		"\2\2\u013c\u0135\3\2\2\2\u013c\u0139\3\2\2\2\u013c\u013b\3\2\2\2\u013d"+
		"=\3\2\2\2\u013e\u013f\5H%\2\u013f\u0142\7\27\2\2\u0140\u0143\5\24\13\2"+
		"\u0141\u0143\5T+\2\u0142\u0140\3\2\2\2\u0142\u0141\3\2\2\2\u0143?\3\2"+
		"\2\2\u0144\u0149\5B\"\2\u0145\u0146\7\30\2\2\u0146\u0148\5B\"\2\u0147"+
		"\u0145\3\2\2\2\u0148\u014b\3\2\2\2\u0149\u0147\3\2\2\2\u0149\u014a\3\2"+
		"\2\2\u014aA\3\2\2\2\u014b\u0149\3\2\2\2\u014c\u014d\b\"\1\2\u014d\u014e"+
		"\7(\2\2\u014e\u014f\5\24\13\2\u014f\u0150\7\26\2\2\u0150\u0151\5B\"\30"+
		"\u0151\u016b\3\2\2\2\u0152\u016b\5D#\2\u0153\u016b\7\63\2\2\u0154\u016b"+
		"\7\64\2\2\u0155\u016b\7\62\2\2\u0156\u016b\5D#\2\u0157\u016b\5V,\2\u0158"+
		"\u0159\7%\2\2\u0159\u016b\5B\"\21\u015a\u015b\7#\2\2\u015b\u016b\5B\""+
		"\20\u015c\u016b\5H%\2\u015d\u015e\5\34\17\2\u015e\u0160\7(\2\2\u015f\u0161"+
		"\5@!\2\u0160\u015f\3\2\2\2\u0160\u0161\3\2\2\2\u0161\u0162\3\2\2\2\u0162"+
		"\u0163\7\26\2\2\u0163\u016b\3\2\2\2\u0164\u0165\7(\2\2\u0165\u0166\5B"+
		"\"\2\u0166\u0167\7\26\2\2\u0167\u016b\3\2\2\2\u0168\u0169\7\n\2\2\u0169"+
		"\u016b\5\22\n\2\u016a\u014c\3\2\2\2\u016a\u0152\3\2\2\2\u016a\u0153\3"+
		"\2\2\2\u016a\u0154\3\2\2\2\u016a\u0155\3\2\2\2\u016a\u0156\3\2\2\2\u016a"+
		"\u0157\3\2\2\2\u016a\u0158\3\2\2\2\u016a\u015a\3\2\2\2\u016a\u015c\3\2"+
		"\2\2\u016a\u015d\3\2\2\2\u016a\u0164\3\2\2\2\u016a\u0168\3\2\2\2\u016b"+
		"\u0189\3\2\2\2\u016c\u016d\f\17\2\2\u016d\u016e\7\"\2\2\u016e\u0188\5"+
		"B\"\20\u016f\u0170\f\16\2\2\u0170\u0171\7\36\2\2\u0171\u0188\5B\"\17\u0172"+
		"\u0173\f\r\2\2\u0173\u0174\7!\2\2\u0174\u0188\5B\"\16\u0175\u0176\f\f"+
		"\2\2\u0176\u0177\7\35\2\2\u0177\u0188\5B\"\r\u0178\u0179\f\13\2\2\u0179"+
		"\u017a\7\21\2\2\u017a\u0188\5B\"\f\u017b\u017c\f\n\2\2\u017c\u017d\7+"+
		"\2\2\u017d\u0188\5B\"\13\u017e\u017f\f\t\2\2\u017f\u0180\7#\2\2\u0180"+
		"\u0188\5B\"\n\u0181\u0182\f\b\2\2\u0182\u0183\7/\2\2\u0183\u0188\5B\""+
		"\t\u0184\u0185\f\7\2\2\u0185\u0186\7\31\2\2\u0186\u0188\5B\"\b\u0187\u016c"+
		"\3\2\2\2\u0187\u016f\3\2\2\2\u0187\u0172\3\2\2\2\u0187\u0175\3\2\2\2\u0187"+
		"\u0178\3\2\2\2\u0187\u017b\3\2\2\2\u0187\u017e\3\2\2\2\u0187\u0181\3\2"+
		"\2\2\u0187\u0184\3\2\2\2\u0188\u018b\3\2\2\2\u0189\u0187\3\2\2\2\u0189"+
		"\u018a\3\2\2\2\u018aC\3\2\2\2\u018b\u0189\3\2\2\2\u018c\u018f\7\r\2\2"+
		"\u018d\u018f\7\7\2\2\u018e\u018c\3\2\2\2\u018e\u018d\3\2\2\2\u018fE\3"+
		"\2\2\2\u0190\u0195\7\61\2\2\u0191\u0192\7\32\2\2\u0192\u0194\7\61\2\2"+
		"\u0193\u0191\3\2\2\2\u0194\u0197\3\2\2\2\u0195\u0193\3\2\2\2\u0195\u0196"+
		"\3\2\2\2\u0196G\3\2\2\2\u0197\u0195\3\2\2\2\u0198\u0199\5F$\2\u0199I\3"+
		"\2\2\2\u019a\u019b\7\3\2\2\u019b\u019c\5\n\6\2\u019c\u019d\7\4\2\2\u019d"+
		"\u019e\5P)\2\u019e\u019f\7.\2\2\u019fK\3\2\2\2\u01a0\u01a3\5N(\2\u01a1"+
		"\u01a3\5P)\2\u01a2\u01a0\3\2\2\2\u01a2\u01a1\3\2\2\2\u01a3M\3\2\2\2\u01a4"+
		"\u01a6\7\61\2\2\u01a5\u01a4\3\2\2\2\u01a5\u01a6\3\2\2\2\u01a6\u01a7\3"+
		"\2\2\2\u01a7\u01a8\7\27\2\2\u01a8\u01a9\7\61\2\2\u01a9O\3\2\2\2\u01aa"+
		"\u01ab\7\61\2\2\u01ab\u01ac\7\27\2\2\u01ac\u01ad\7\31\2\2\u01ad\u01ae"+
		"\7\31\2\2\u01ae\u01b3\7\61\2\2\u01af\u01b0\7\32\2\2\u01b0\u01b2\7\61\2"+
		"\2\u01b1\u01af\3\2\2\2\u01b2\u01b5\3\2\2\2\u01b3\u01b1\3\2\2\2\u01b3\u01b4"+
		"\3\2\2\2\u01b4\u01ba\3\2\2\2\u01b5\u01b3\3\2\2\2\u01b6\u01b7\7\31\2\2"+
		"\u01b7\u01b9\7\61\2\2\u01b8\u01b6\3\2\2\2\u01b9\u01bc\3\2\2\2\u01ba\u01b8"+
		"\3\2\2\2\u01ba\u01bb\3\2\2\2\u01bb\u01be\3\2\2\2\u01bc\u01ba\3\2\2\2\u01bd"+
		"\u01bf\7\31\2\2\u01be\u01bd\3\2\2\2\u01be\u01bf\3\2\2\2\u01bf\u01c4\3"+
		"\2\2\2\u01c0\u01c2\7\37\2\2\u01c1\u01c3\7\61\2\2\u01c2\u01c1\3\2\2\2\u01c2"+
		"\u01c3\3\2\2\2\u01c3\u01c5\3\2\2\2\u01c4\u01c0\3\2\2\2\u01c4\u01c5\3\2"+
		"\2\2\u01c5Q\3\2\2\2\u01c6\u01c7\7\61\2\2\u01c7\u01c8\7\22\2\2\u01c8\u01c9"+
		"\7\61\2\2\u01c9S\3\2\2\2\u01ca\u01cb\5\24\13\2\u01cb\u01cc\7\'\2\2\u01cc"+
		"\u01cd\7\25\2\2\u01cdU\3\2\2\2\u01ce\u01cf\7\'\2\2\u01cf\u01d0\5X-\2\u01d0"+
		"\u01d1\7\25\2\2\u01d1W\3\2\2\2\u01d2\u01d5\5Z.\2\u01d3\u01d5\5\\/\2\u01d4"+
		"\u01d2\3\2\2\2\u01d4\u01d3\3\2\2\2\u01d5Y\3\2\2\2\u01d6\u01d7\5@!\2\u01d7"+
		"[\3\2\2\2\u01d8\u01d9\5H%\2\u01d9\u01da\7\23\2\2\u01da\u01db\5^\60\2\u01db"+
		"\u01dc\7\30\2\2\u01dc\u01dd\5`\61\2\u01dd]\3\2\2\2\u01de\u01df\5H%\2\u01df"+
		"\u01e0\7\34\2\2\u01e0\u01e1\5H%\2\u01e1_\3\2\2\2\u01e2\u01e3\5B\"\2\u01e3"+
		"a\3\2\2\2)ekpr|\u0087\u0095\u0097\u00ab\u00c7\u00cc\u00d4\u00d7\u00e2"+
		"\u00eb\u00f8\u00fb\u0107\u0110\u0118\u0124\u0133\u013c\u0142\u0149\u0160"+
		"\u016a\u0187\u0189\u018e\u0195\u01a2\u01a5\u01b3\u01ba\u01be\u01c2\u01c4"+
		"\u01d4";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}