// Generated from c:/dev/aabs/5th-related/ast-builder/src/parser/grammar/Fifth.g4 by ANTLR 4.13.1
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast", "CheckReturnValue"})
public class FifthParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.13.1", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		T__0=1, T__1=2, T__2=3, ALIAS=4, AS=5, CLASS=6, ELSE=7, FALSE=8, IF=9, 
		LIST=10, NEW=11, RETURN=12, USE=13, TRUE=14, WHILE=15, WITH=16, TN_SHORT=17, 
		TN_INT=18, TN_LONG=19, TN_FLOAT=20, TN_DOUBLE=21, TN_DECIMAL=22, OP_LogicalNot=23, 
		OP_ArithmeticAdd=24, OP_StringConcatenate=25, OP_ArithmeticNegative=26, 
		OP_ArithmeticSubtract=27, OP_ArithmeticMultiply=28, OP_ArithmeticDivide=29, 
		OP_ArithmeticRem=30, OP_ArithmeticMod=31, OP_ArithmeticPow=32, OP_BitwiseAnd=33, 
		OP_BitwiseOr=34, OP_LogicalAnd=35, OP_LogicalOr=36, OP_LogicalNand=37, 
		OP_LogicalNor=38, OP_LogicalXor=39, OP_Equal=40, OP_NotEqual=41, OP_LessThan=42, 
		OP_GreaterThan=43, OP_LessThanOrEqual=44, OP_GreaterThanOrEqual=45, OP_BitwiseLeftShift=46, 
		OP_BitwiseRightShift=47, UNIOP=48, BINOP=49, LAMBDASEP=50, HASH=51, DOT=52, 
		GEN=53, COLON=54, SEMICOLON=55, COMMA=56, ASSIGN=57, BAR=58, CLOSEBRACE=59, 
		CLOSEBRACK=60, CLOSEPAREN=61, OPENBRACE=62, OPENBRACK=63, OPENPAREN=64, 
		QMARK=65, UNDERSCORE=66, IDENTIFIER=67, STRING=68, INT=69, FLOAT=70, WS=71, 
		DIVIDE=72;
	public static final int
		RULE_call_site = 0, RULE_fifth = 1, RULE_function_call = 2, RULE_member_access_expression = 3, 
		RULE_module_import = 4, RULE_module_name = 5, RULE_packagename = 6, RULE_class_definition = 7, 
		RULE_property_declaration = 8, RULE_type_initialiser = 9, RULE_type_name = 10, 
		RULE_type_property_init = 11, RULE_function_declaration = 12, RULE_function_body = 13, 
		RULE_function_name = 14, RULE_function_type = 15, RULE_variable_constraint = 16, 
		RULE_paramdecl = 17, RULE_param_name = 18, RULE_param_type = 19, RULE_destructuring_decl = 20, 
		RULE_destructure_binding = 21, RULE_block = 22, RULE_statement = 23, RULE_var_decl = 24, 
		RULE_identifier_chain = 25, RULE_explist = 26, RULE_literal_exp = 27, 
		RULE_unary_exp = 28, RULE_mult_exp = 29, RULE_add_exp = 30, RULE_exp = 31, 
		RULE_truth_value = 32, RULE_absoluteIri = 33, RULE_alias = 34, RULE_iri = 35, 
		RULE_iri_query_param = 36, RULE_qNameIri = 37, RULE_list = 38, RULE_list_body = 39, 
		RULE_list_comp_constraint = 40, RULE_list_comp_generator = 41, RULE_list_literal = 42, 
		RULE_list_comprehension = 43, RULE_list_type_signature = 44, RULE_var_name = 45;
	private static String[] makeRuleNames() {
		return new String[] {
			"call_site", "fifth", "function_call", "member_access_expression", "module_import", 
			"module_name", "packagename", "class_definition", "property_declaration", 
			"type_initialiser", "type_name", "type_property_init", "function_declaration", 
			"function_body", "function_name", "function_type", "variable_constraint", 
			"paramdecl", "param_name", "param_type", "destructuring_decl", "destructure_binding", 
			"block", "statement", "var_decl", "identifier_chain", "explist", "literal_exp", 
			"unary_exp", "mult_exp", "add_exp", "exp", "truth_value", "absoluteIri", 
			"alias", "iri", "iri_query_param", "qNameIri", "list", "list_body", "list_comp_constraint", 
			"list_comp_generator", "list_literal", "list_comprehension", "list_type_signature", 
			"var_name"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'s'", "'l'", "'d'", "'alias'", "'as'", "'class'", "'else'", "'false'", 
			"'if'", "'list'", "'new'", "'return'", "'use'", "'true'", "'while'", 
			"'with'", "'short'", "'int'", "'long'", "'float'", "'double'", "'decimal'", 
			"'!'", null, null, null, null, "'*'", "'/'", null, "'%'", "'**'", "'&'", 
			null, "'&&'", "'||'", "'!&'", "'!|'", "'^'", "'=='", "'!='", "'<'", "'>'", 
			"'<='", "'>='", "'<<'", "'>>'", null, null, "'=>'", null, "'.'", "'<-'", 
			"':'", "';'", "','", "'='", null, "'}'", "']'", "')'", "'{'", "'['", 
			"'('", "'?'", "'_'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, null, null, null, "ALIAS", "AS", "CLASS", "ELSE", "FALSE", "IF", 
			"LIST", "NEW", "RETURN", "USE", "TRUE", "WHILE", "WITH", "TN_SHORT", 
			"TN_INT", "TN_LONG", "TN_FLOAT", "TN_DOUBLE", "TN_DECIMAL", "OP_LogicalNot", 
			"OP_ArithmeticAdd", "OP_StringConcatenate", "OP_ArithmeticNegative", 
			"OP_ArithmeticSubtract", "OP_ArithmeticMultiply", "OP_ArithmeticDivide", 
			"OP_ArithmeticRem", "OP_ArithmeticMod", "OP_ArithmeticPow", "OP_BitwiseAnd", 
			"OP_BitwiseOr", "OP_LogicalAnd", "OP_LogicalOr", "OP_LogicalNand", "OP_LogicalNor", 
			"OP_LogicalXor", "OP_Equal", "OP_NotEqual", "OP_LessThan", "OP_GreaterThan", 
			"OP_LessThanOrEqual", "OP_GreaterThanOrEqual", "OP_BitwiseLeftShift", 
			"OP_BitwiseRightShift", "UNIOP", "BINOP", "LAMBDASEP", "HASH", "DOT", 
			"GEN", "COLON", "SEMICOLON", "COMMA", "ASSIGN", "BAR", "CLOSEBRACE", 
			"CLOSEBRACK", "CLOSEPAREN", "OPENBRACE", "OPENBRACK", "OPENPAREN", "QMARK", 
			"UNDERSCORE", "IDENTIFIER", "STRING", "INT", "FLOAT", "WS", "DIVIDE"
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

	@SuppressWarnings("CheckReturnValue")
	public static class Call_siteContext extends ParserRuleContext {
		public Call_siteContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_call_site; }
	 
		public Call_siteContext() { }
		public void copyFrom(Call_siteContext ctx) {
			super.copyFrom(ctx);
		}
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_callsite_varnameContext extends Call_siteContext {
		public Var_nameContext var_name() {
			return getRuleContext(Var_nameContext.class,0);
		}
		public Exp_callsite_varnameContext(Call_siteContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_callsite_func_callContext extends Call_siteContext {
		public Function_callContext function_call() {
			return getRuleContext(Function_callContext.class,0);
		}
		public Exp_callsite_func_callContext(Call_siteContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_callsite_parenthesisedContext extends Call_siteContext {
		public ExpContext innerexp;
		public TerminalNode OPENPAREN() { return getToken(FifthParser.OPENPAREN, 0); }
		public TerminalNode CLOSEPAREN() { return getToken(FifthParser.CLOSEPAREN, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public Exp_callsite_parenthesisedContext(Call_siteContext ctx) { copyFrom(ctx); }
	}

	public final Call_siteContext call_site() throws RecognitionException {
		Call_siteContext _localctx = new Call_siteContext(_ctx, getState());
		enterRule(_localctx, 0, RULE_call_site);
		try {
			setState(98);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,0,_ctx) ) {
			case 1:
				_localctx = new Exp_callsite_varnameContext(_localctx);
				enterOuterAlt(_localctx, 1);
				{
				setState(92);
				var_name();
				}
				break;
			case 2:
				_localctx = new Exp_callsite_func_callContext(_localctx);
				enterOuterAlt(_localctx, 2);
				{
				setState(93);
				function_call();
				}
				break;
			case 3:
				_localctx = new Exp_callsite_parenthesisedContext(_localctx);
				enterOuterAlt(_localctx, 3);
				{
				setState(94);
				match(OPENPAREN);
				setState(95);
				((Exp_callsite_parenthesisedContext)_localctx).innerexp = exp();
				setState(96);
				match(CLOSEPAREN);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 2, RULE_fifth);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(103);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==USE) {
				{
				{
				setState(100);
				module_import();
				}
				}
				setState(105);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(109);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==ALIAS) {
				{
				{
				setState(106);
				alias();
				}
				}
				setState(111);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(116);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==CLASS || _la==IDENTIFIER) {
				{
				setState(114);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case IDENTIFIER:
					{
					setState(112);
					((FifthContext)_localctx).function_declaration = function_declaration();
					((FifthContext)_localctx).functions.add(((FifthContext)_localctx).function_declaration);
					}
					break;
				case CLASS:
					{
					setState(113);
					((FifthContext)_localctx).class_definition = class_definition();
					((FifthContext)_localctx).classes.add(((FifthContext)_localctx).class_definition);
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				}
				setState(118);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 4, RULE_function_call);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(119);
			function_name();
			setState(120);
			match(OPENPAREN);
			setState(121);
			exp();
			setState(126);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(122);
				match(COMMA);
				setState(123);
				exp();
				}
				}
				setState(128);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(129);
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

	@SuppressWarnings("CheckReturnValue")
	public static class Member_access_expressionContext extends ParserRuleContext {
		public Call_siteContext call_site;
		public List<Call_siteContext> segments = new ArrayList<Call_siteContext>();
		public List<Call_siteContext> call_site() {
			return getRuleContexts(Call_siteContext.class);
		}
		public Call_siteContext call_site(int i) {
			return getRuleContext(Call_siteContext.class,i);
		}
		public List<TerminalNode> DOT() { return getTokens(FifthParser.DOT); }
		public TerminalNode DOT(int i) {
			return getToken(FifthParser.DOT, i);
		}
		public Member_access_expressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_member_access_expression; }
	}

	public final Member_access_expressionContext member_access_expression() throws RecognitionException {
		Member_access_expressionContext _localctx = new Member_access_expressionContext(_ctx, getState());
		enterRule(_localctx, 6, RULE_member_access_expression);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(131);
			((Member_access_expressionContext)_localctx).call_site = call_site();
			((Member_access_expressionContext)_localctx).segments.add(((Member_access_expressionContext)_localctx).call_site);
			setState(136);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==DOT) {
				{
				{
				setState(132);
				match(DOT);
				setState(133);
				((Member_access_expressionContext)_localctx).call_site = call_site();
				((Member_access_expressionContext)_localctx).segments.add(((Member_access_expressionContext)_localctx).call_site);
				}
				}
				setState(138);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 8, RULE_module_import);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(139);
			match(USE);
			setState(140);
			module_name();
			setState(145);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(141);
				match(COMMA);
				setState(142);
				module_name();
				}
				}
				setState(147);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(148);
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

	@SuppressWarnings("CheckReturnValue")
	public static class Module_nameContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public Module_nameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_module_name; }
	}

	public final Module_nameContext module_name() throws RecognitionException {
		Module_nameContext _localctx = new Module_nameContext(_ctx, getState());
		enterRule(_localctx, 10, RULE_module_name);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(150);
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

	@SuppressWarnings("CheckReturnValue")
	public static class PackagenameContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public PackagenameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_packagename; }
	}

	public final PackagenameContext packagename() throws RecognitionException {
		PackagenameContext _localctx = new PackagenameContext(_ctx, getState());
		enterRule(_localctx, 12, RULE_packagename);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(152);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 14, RULE_class_definition);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(154);
			match(CLASS);
			setState(155);
			((Class_definitionContext)_localctx).name = match(IDENTIFIER);
			setState(156);
			match(OPENBRACE);
			setState(161);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==IDENTIFIER) {
				{
				setState(159);
				_errHandler.sync(this);
				switch ( getInterpreter().adaptivePredict(_input,8,_ctx) ) {
				case 1:
					{
					setState(157);
					((Class_definitionContext)_localctx).function_declaration = function_declaration();
					((Class_definitionContext)_localctx).functions.add(((Class_definitionContext)_localctx).function_declaration);
					}
					break;
				case 2:
					{
					setState(158);
					((Class_definitionContext)_localctx).property_declaration = property_declaration();
					((Class_definitionContext)_localctx).properties.add(((Class_definitionContext)_localctx).property_declaration);
					}
					break;
				}
				}
				setState(163);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(164);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 16, RULE_property_declaration);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(166);
			((Property_declarationContext)_localctx).name = match(IDENTIFIER);
			setState(167);
			match(COLON);
			setState(168);
			((Property_declarationContext)_localctx).type = match(IDENTIFIER);
			setState(169);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 18, RULE_type_initialiser);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(171);
			((Type_initialiserContext)_localctx).typename = type_name();
			setState(172);
			match(OPENBRACE);
			setState(173);
			((Type_initialiserContext)_localctx).type_property_init = type_property_init();
			((Type_initialiserContext)_localctx).properties.add(((Type_initialiserContext)_localctx).type_property_init);
			setState(178);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(174);
				match(COMMA);
				setState(175);
				((Type_initialiserContext)_localctx).type_property_init = type_property_init();
				((Type_initialiserContext)_localctx).properties.add(((Type_initialiserContext)_localctx).type_property_init);
				}
				}
				setState(180);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(181);
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

	@SuppressWarnings("CheckReturnValue")
	public static class Type_nameContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public Type_nameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_type_name; }
	}

	public final Type_nameContext type_name() throws RecognitionException {
		Type_nameContext _localctx = new Type_nameContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_type_name);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(183);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 22, RULE_type_property_init);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(185);
			var_name();
			setState(186);
			match(ASSIGN);
			setState(187);
			exp();
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

	@SuppressWarnings("CheckReturnValue")
	public static class Function_declarationContext extends ParserRuleContext {
		public Function_nameContext name;
		public ParamdeclContext paramdecl;
		public List<ParamdeclContext> args = new ArrayList<ParamdeclContext>();
		public Function_typeContext result_type;
		public Function_bodyContext body;
		public TerminalNode OPENPAREN() { return getToken(FifthParser.OPENPAREN, 0); }
		public TerminalNode CLOSEPAREN() { return getToken(FifthParser.CLOSEPAREN, 0); }
		public TerminalNode COLON() { return getToken(FifthParser.COLON, 0); }
		public Function_nameContext function_name() {
			return getRuleContext(Function_nameContext.class,0);
		}
		public Function_typeContext function_type() {
			return getRuleContext(Function_typeContext.class,0);
		}
		public Function_bodyContext function_body() {
			return getRuleContext(Function_bodyContext.class,0);
		}
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
		public Function_declarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_function_declaration; }
	}

	public final Function_declarationContext function_declaration() throws RecognitionException {
		Function_declarationContext _localctx = new Function_declarationContext(_ctx, getState());
		enterRule(_localctx, 24, RULE_function_declaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(189);
			((Function_declarationContext)_localctx).name = function_name();
			setState(190);
			match(OPENPAREN);
			setState(199);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==IDENTIFIER) {
				{
				setState(191);
				((Function_declarationContext)_localctx).paramdecl = paramdecl();
				((Function_declarationContext)_localctx).args.add(((Function_declarationContext)_localctx).paramdecl);
				setState(196);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==COMMA) {
					{
					{
					setState(192);
					match(COMMA);
					setState(193);
					((Function_declarationContext)_localctx).paramdecl = paramdecl();
					((Function_declarationContext)_localctx).args.add(((Function_declarationContext)_localctx).paramdecl);
					}
					}
					setState(198);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
			}

			setState(201);
			match(CLOSEPAREN);
			setState(202);
			match(COLON);
			setState(203);
			((Function_declarationContext)_localctx).result_type = function_type();
			setState(204);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 26, RULE_function_body);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(206);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 28, RULE_function_name);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(208);
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

	@SuppressWarnings("CheckReturnValue")
	public static class Function_typeContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public Function_typeContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_function_type; }
	}

	public final Function_typeContext function_type() throws RecognitionException {
		Function_typeContext _localctx = new Function_typeContext(_ctx, getState());
		enterRule(_localctx, 30, RULE_function_type);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(210);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 32, RULE_variable_constraint);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(212);
			match(BAR);
			setState(213);
			((Variable_constraintContext)_localctx).constraint = exp();
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

	@SuppressWarnings("CheckReturnValue")
	public static class ParamdeclContext extends ParserRuleContext {
		public Param_nameContext param_name() {
			return getRuleContext(Param_nameContext.class,0);
		}
		public TerminalNode COLON() { return getToken(FifthParser.COLON, 0); }
		public Param_typeContext param_type() {
			return getRuleContext(Param_typeContext.class,0);
		}
		public Variable_constraintContext variable_constraint() {
			return getRuleContext(Variable_constraintContext.class,0);
		}
		public Destructuring_declContext destructuring_decl() {
			return getRuleContext(Destructuring_declContext.class,0);
		}
		public ParamdeclContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_paramdecl; }
	}

	public final ParamdeclContext paramdecl() throws RecognitionException {
		ParamdeclContext _localctx = new ParamdeclContext(_ctx, getState());
		enterRule(_localctx, 34, RULE_paramdecl);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(215);
			param_name();
			setState(216);
			match(COLON);
			setState(217);
			param_type();
			setState(220);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BAR:
				{
				setState(218);
				variable_constraint();
				}
				break;
			case OPENBRACE:
				{
				setState(219);
				destructuring_decl();
				}
				break;
			case COMMA:
			case CLOSEPAREN:
				break;
			default:
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

	@SuppressWarnings("CheckReturnValue")
	public static class Param_nameContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public Param_nameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_param_name; }
	}

	public final Param_nameContext param_name() throws RecognitionException {
		Param_nameContext _localctx = new Param_nameContext(_ctx, getState());
		enterRule(_localctx, 36, RULE_param_name);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(222);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 38, RULE_param_type);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(224);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 40, RULE_destructuring_decl);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(226);
			match(OPENBRACE);
			setState(227);
			((Destructuring_declContext)_localctx).destructure_binding = destructure_binding();
			((Destructuring_declContext)_localctx).bindings.add(((Destructuring_declContext)_localctx).destructure_binding);
			setState(232);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(228);
				match(COMMA);
				setState(229);
				((Destructuring_declContext)_localctx).destructure_binding = destructure_binding();
				((Destructuring_declContext)_localctx).bindings.add(((Destructuring_declContext)_localctx).destructure_binding);
				}
				}
				setState(234);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(235);
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

	@SuppressWarnings("CheckReturnValue")
	public static class Destructure_bindingContext extends ParserRuleContext {
		public Token name;
		public Token propname;
		public TerminalNode COLON() { return getToken(FifthParser.COLON, 0); }
		public List<TerminalNode> IDENTIFIER() { return getTokens(FifthParser.IDENTIFIER); }
		public TerminalNode IDENTIFIER(int i) {
			return getToken(FifthParser.IDENTIFIER, i);
		}
		public Variable_constraintContext variable_constraint() {
			return getRuleContext(Variable_constraintContext.class,0);
		}
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
		enterRule(_localctx, 42, RULE_destructure_binding);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(237);
			((Destructure_bindingContext)_localctx).name = match(IDENTIFIER);
			setState(238);
			match(COLON);
			setState(239);
			((Destructure_bindingContext)_localctx).propname = match(IDENTIFIER);
			setState(242);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BAR:
				{
				setState(240);
				variable_constraint();
				}
				break;
			case OPENBRACE:
				{
				setState(241);
				destructuring_decl();
				}
				break;
			case COMMA:
			case CLOSEBRACE:
				break;
			default:
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 44, RULE_block);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(244);
			match(OPENBRACE);
			setState(250);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (((((_la - 8)) & ~0x3f) == 0 && ((1L << (_la - 8)) & 8754998775119872475L) != 0)) {
				{
				{
				setState(245);
				statement();
				setState(246);
				match(SEMICOLON);
				}
				}
				setState(252);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(253);
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

	@SuppressWarnings("CheckReturnValue")
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
	@SuppressWarnings("CheckReturnValue")
	public static class Stmt_ifelseContext extends StatementContext {
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
		public Stmt_ifelseContext(StatementContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Stmt_assignmentContext extends StatementContext {
		public Var_nameContext var_name() {
			return getRuleContext(Var_nameContext.class,0);
		}
		public TerminalNode ASSIGN() { return getToken(FifthParser.ASSIGN, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public Stmt_assignmentContext(StatementContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Stmt_whileContext extends StatementContext {
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
		public Stmt_whileContext(StatementContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Stmt_vardeclContext extends StatementContext {
		public Var_declContext decl;
		public Var_declContext var_decl() {
			return getRuleContext(Var_declContext.class,0);
		}
		public TerminalNode ASSIGN() { return getToken(FifthParser.ASSIGN, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public Stmt_vardeclContext(StatementContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Stmt_returnContext extends StatementContext {
		public TerminalNode RETURN() { return getToken(FifthParser.RETURN, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public Stmt_returnContext(StatementContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Stmt_bareexpressionContext extends StatementContext {
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public Stmt_bareexpressionContext(StatementContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Stmt_withContext extends StatementContext {
		public TerminalNode WITH() { return getToken(FifthParser.WITH, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public BlockContext block() {
			return getRuleContext(BlockContext.class,0);
		}
		public Stmt_withContext(StatementContext ctx) { copyFrom(ctx); }
	}

	public final StatementContext statement() throws RecognitionException {
		StatementContext _localctx = new StatementContext(_ctx, getState());
		enterRule(_localctx, 46, RULE_statement);
		int _la;
		try {
			setState(286);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,19,_ctx) ) {
			case 1:
				_localctx = new Stmt_ifelseContext(_localctx);
				enterOuterAlt(_localctx, 1);
				{
				setState(255);
				match(IF);
				setState(256);
				match(OPENPAREN);
				setState(257);
				((Stmt_ifelseContext)_localctx).condition = exp();
				setState(258);
				match(CLOSEPAREN);
				setState(259);
				((Stmt_ifelseContext)_localctx).ifpart = block();
				setState(262);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==ELSE) {
					{
					setState(260);
					match(ELSE);
					setState(261);
					((Stmt_ifelseContext)_localctx).elsepart = block();
					}
				}

				}
				break;
			case 2:
				_localctx = new Stmt_whileContext(_localctx);
				enterOuterAlt(_localctx, 2);
				{
				setState(264);
				match(WHILE);
				setState(265);
				match(OPENPAREN);
				setState(266);
				((Stmt_whileContext)_localctx).condition = exp();
				setState(267);
				match(CLOSEPAREN);
				setState(268);
				((Stmt_whileContext)_localctx).looppart = block();
				}
				break;
			case 3:
				_localctx = new Stmt_withContext(_localctx);
				enterOuterAlt(_localctx, 3);
				{
				setState(270);
				match(WITH);
				setState(271);
				exp();
				setState(272);
				block();
				}
				break;
			case 4:
				_localctx = new Stmt_vardeclContext(_localctx);
				enterOuterAlt(_localctx, 4);
				{
				setState(274);
				((Stmt_vardeclContext)_localctx).decl = var_decl();
				setState(277);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==ASSIGN) {
					{
					setState(275);
					match(ASSIGN);
					setState(276);
					exp();
					}
				}

				}
				break;
			case 5:
				_localctx = new Stmt_assignmentContext(_localctx);
				enterOuterAlt(_localctx, 5);
				{
				setState(279);
				var_name();
				setState(280);
				match(ASSIGN);
				setState(281);
				exp();
				}
				break;
			case 6:
				_localctx = new Stmt_returnContext(_localctx);
				enterOuterAlt(_localctx, 6);
				{
				setState(283);
				match(RETURN);
				setState(284);
				exp();
				}
				break;
			case 7:
				_localctx = new Stmt_bareexpressionContext(_localctx);
				enterOuterAlt(_localctx, 7);
				{
				setState(285);
				exp();
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 48, RULE_var_decl);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(288);
			var_name();
			setState(289);
			match(COLON);
			setState(292);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,20,_ctx) ) {
			case 1:
				{
				setState(290);
				type_name();
				}
				break;
			case 2:
				{
				setState(291);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 50, RULE_identifier_chain);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(294);
			((Identifier_chainContext)_localctx).IDENTIFIER = match(IDENTIFIER);
			((Identifier_chainContext)_localctx).segments.add(((Identifier_chainContext)_localctx).IDENTIFIER);
			setState(299);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==DOT) {
				{
				{
				setState(295);
				match(DOT);
				setState(296);
				((Identifier_chainContext)_localctx).IDENTIFIER = match(IDENTIFIER);
				((Identifier_chainContext)_localctx).segments.add(((Identifier_chainContext)_localctx).IDENTIFIER);
				}
				}
				setState(301);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 52, RULE_explist);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(302);
			exp();
			setState(307);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(303);
				match(COMMA);
				setState(304);
				exp();
				}
				}
				setState(309);
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

	@SuppressWarnings("CheckReturnValue")
	public static class Literal_expContext extends ParserRuleContext {
		public Literal_expContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_literal_exp; }
	 
		public Literal_expContext() { }
		public void copyFrom(Literal_expContext ctx) {
			super.copyFrom(ctx);
		}
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_longContext extends Literal_expContext {
		public Token value;
		public TerminalNode INT() { return getToken(FifthParser.INT, 0); }
		public Exp_longContext(Literal_expContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_shortContext extends Literal_expContext {
		public Token value;
		public TerminalNode INT() { return getToken(FifthParser.INT, 0); }
		public Exp_shortContext(Literal_expContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_stringContext extends Literal_expContext {
		public Token value;
		public TerminalNode STRING() { return getToken(FifthParser.STRING, 0); }
		public Exp_stringContext(Literal_expContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_boolContext extends Literal_expContext {
		public Truth_valueContext value;
		public Truth_valueContext truth_value() {
			return getRuleContext(Truth_valueContext.class,0);
		}
		public Exp_boolContext(Literal_expContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_varnameContext extends Literal_expContext {
		public Var_nameContext var_name() {
			return getRuleContext(Var_nameContext.class,0);
		}
		public Exp_varnameContext(Literal_expContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_floatContext extends Literal_expContext {
		public Token value;
		public TerminalNode FLOAT() { return getToken(FifthParser.FLOAT, 0); }
		public Exp_floatContext(Literal_expContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_doubleContext extends Literal_expContext {
		public Token value;
		public TerminalNode FLOAT() { return getToken(FifthParser.FLOAT, 0); }
		public Exp_doubleContext(Literal_expContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_intContext extends Literal_expContext {
		public Token value;
		public TerminalNode INT() { return getToken(FifthParser.INT, 0); }
		public Exp_intContext(Literal_expContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_listContext extends Literal_expContext {
		public ListContext value;
		public ListContext list() {
			return getRuleContext(ListContext.class,0);
		}
		public Exp_listContext(Literal_expContext ctx) { copyFrom(ctx); }
	}

	public final Literal_expContext literal_exp() throws RecognitionException {
		Literal_expContext _localctx = new Literal_expContext(_ctx, getState());
		enterRule(_localctx, 54, RULE_literal_exp);
		try {
			setState(322);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,23,_ctx) ) {
			case 1:
				_localctx = new Exp_boolContext(_localctx);
				enterOuterAlt(_localctx, 1);
				{
				setState(310);
				((Exp_boolContext)_localctx).value = truth_value();
				}
				break;
			case 2:
				_localctx = new Exp_shortContext(_localctx);
				enterOuterAlt(_localctx, 2);
				{
				setState(311);
				((Exp_shortContext)_localctx).value = match(INT);
				setState(312);
				match(T__0);
				}
				break;
			case 3:
				_localctx = new Exp_intContext(_localctx);
				enterOuterAlt(_localctx, 3);
				{
				setState(313);
				((Exp_intContext)_localctx).value = match(INT);
				}
				break;
			case 4:
				_localctx = new Exp_longContext(_localctx);
				enterOuterAlt(_localctx, 4);
				{
				setState(314);
				((Exp_longContext)_localctx).value = match(INT);
				setState(315);
				match(T__1);
				}
				break;
			case 5:
				_localctx = new Exp_floatContext(_localctx);
				enterOuterAlt(_localctx, 5);
				{
				setState(316);
				((Exp_floatContext)_localctx).value = match(FLOAT);
				}
				break;
			case 6:
				_localctx = new Exp_doubleContext(_localctx);
				enterOuterAlt(_localctx, 6);
				{
				setState(317);
				((Exp_doubleContext)_localctx).value = match(FLOAT);
				setState(318);
				match(T__2);
				}
				break;
			case 7:
				_localctx = new Exp_stringContext(_localctx);
				enterOuterAlt(_localctx, 7);
				{
				setState(319);
				((Exp_stringContext)_localctx).value = match(STRING);
				}
				break;
			case 8:
				_localctx = new Exp_listContext(_localctx);
				enterOuterAlt(_localctx, 8);
				{
				setState(320);
				((Exp_listContext)_localctx).value = list();
				}
				break;
			case 9:
				_localctx = new Exp_varnameContext(_localctx);
				enterOuterAlt(_localctx, 9);
				{
				setState(321);
				var_name();
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

	@SuppressWarnings("CheckReturnValue")
	public static class Unary_expContext extends ParserRuleContext {
		public Token op;
		public Literal_expContext literal_exp() {
			return getRuleContext(Literal_expContext.class,0);
		}
		public TerminalNode UNIOP() { return getToken(FifthParser.UNIOP, 0); }
		public Unary_expContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_unary_exp; }
	}

	public final Unary_expContext unary_exp() throws RecognitionException {
		Unary_expContext _localctx = new Unary_expContext(_ctx, getState());
		enterRule(_localctx, 56, RULE_unary_exp);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(325);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==UNIOP) {
				{
				setState(324);
				((Unary_expContext)_localctx).op = match(UNIOP);
				}
			}

			setState(327);
			literal_exp();
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

	@SuppressWarnings("CheckReturnValue")
	public static class Mult_expContext extends ParserRuleContext {
		public Unary_expContext lhs;
		public Token op;
		public Unary_expContext rhs;
		public List<Unary_expContext> unary_exp() {
			return getRuleContexts(Unary_expContext.class);
		}
		public Unary_expContext unary_exp(int i) {
			return getRuleContext(Unary_expContext.class,i);
		}
		public TerminalNode OP_ArithmeticMultiply() { return getToken(FifthParser.OP_ArithmeticMultiply, 0); }
		public TerminalNode OP_ArithmeticDivide() { return getToken(FifthParser.OP_ArithmeticDivide, 0); }
		public Mult_expContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_mult_exp; }
	}

	public final Mult_expContext mult_exp() throws RecognitionException {
		Mult_expContext _localctx = new Mult_expContext(_ctx, getState());
		enterRule(_localctx, 58, RULE_mult_exp);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(329);
			((Mult_expContext)_localctx).lhs = unary_exp();
			setState(332);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==OP_ArithmeticMultiply || _la==OP_ArithmeticDivide) {
				{
				setState(330);
				((Mult_expContext)_localctx).op = _input.LT(1);
				_la = _input.LA(1);
				if ( !(_la==OP_ArithmeticMultiply || _la==OP_ArithmeticDivide) ) {
					((Mult_expContext)_localctx).op = (Token)_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(331);
				((Mult_expContext)_localctx).rhs = unary_exp();
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

	@SuppressWarnings("CheckReturnValue")
	public static class Add_expContext extends ParserRuleContext {
		public Mult_expContext lhs;
		public Token op;
		public Mult_expContext rhs;
		public List<Mult_expContext> mult_exp() {
			return getRuleContexts(Mult_expContext.class);
		}
		public Mult_expContext mult_exp(int i) {
			return getRuleContext(Mult_expContext.class,i);
		}
		public TerminalNode BINOP() { return getToken(FifthParser.BINOP, 0); }
		public Add_expContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_add_exp; }
	}

	public final Add_expContext add_exp() throws RecognitionException {
		Add_expContext _localctx = new Add_expContext(_ctx, getState());
		enterRule(_localctx, 60, RULE_add_exp);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(334);
			((Add_expContext)_localctx).lhs = mult_exp();
			setState(337);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==BINOP) {
				{
				setState(335);
				((Add_expContext)_localctx).op = match(BINOP);
				setState(336);
				((Add_expContext)_localctx).rhs = mult_exp();
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

	@SuppressWarnings("CheckReturnValue")
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
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_parenContext extends ExpContext {
		public ExpContext innerexp;
		public TerminalNode OPENPAREN() { return getToken(FifthParser.OPENPAREN, 0); }
		public TerminalNode CLOSEPAREN() { return getToken(FifthParser.CLOSEPAREN, 0); }
		public ExpContext exp() {
			return getRuleContext(ExpContext.class,0);
		}
		public Exp_parenContext(ExpContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_memberaccessContext extends ExpContext {
		public Member_access_expressionContext member_access_expression() {
			return getRuleContext(Member_access_expressionContext.class,0);
		}
		public Exp_memberaccessContext(ExpContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_addexpContext extends ExpContext {
		public Add_expContext add_exp() {
			return getRuleContext(Add_expContext.class,0);
		}
		public Exp_addexpContext(ExpContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_typecastContext extends ExpContext {
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
		public Exp_typecastContext(ExpContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_funccallContext extends ExpContext {
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
		public Exp_funccallContext(ExpContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class Exp_typecreateinstContext extends ExpContext {
		public TerminalNode NEW() { return getToken(FifthParser.NEW, 0); }
		public Type_initialiserContext type_initialiser() {
			return getRuleContext(Type_initialiserContext.class,0);
		}
		public Exp_typecreateinstContext(ExpContext ctx) { copyFrom(ctx); }
	}

	public final ExpContext exp() throws RecognitionException {
		ExpContext _localctx = new ExpContext(_ctx, getState());
		enterRule(_localctx, 62, RULE_exp);
		int _la;
		try {
			setState(359);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,28,_ctx) ) {
			case 1:
				_localctx = new Exp_parenContext(_localctx);
				enterOuterAlt(_localctx, 1);
				{
				setState(339);
				match(OPENPAREN);
				setState(340);
				((Exp_parenContext)_localctx).innerexp = exp();
				setState(341);
				match(CLOSEPAREN);
				}
				break;
			case 2:
				_localctx = new Exp_typecastContext(_localctx);
				enterOuterAlt(_localctx, 2);
				{
				setState(343);
				match(OPENPAREN);
				setState(344);
				((Exp_typecastContext)_localctx).type = type_name();
				setState(345);
				match(CLOSEPAREN);
				setState(346);
				((Exp_typecastContext)_localctx).subexp = exp();
				}
				break;
			case 3:
				_localctx = new Exp_addexpContext(_localctx);
				enterOuterAlt(_localctx, 3);
				{
				setState(348);
				add_exp();
				}
				break;
			case 4:
				_localctx = new Exp_funccallContext(_localctx);
				enterOuterAlt(_localctx, 4);
				{
				setState(349);
				((Exp_funccallContext)_localctx).funcname = function_name();
				setState(350);
				match(OPENPAREN);
				setState(352);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (((((_la - 8)) & ~0x3f) == 0 && ((1L << (_la - 8)) & 8754998775119872073L) != 0)) {
					{
					setState(351);
					((Exp_funccallContext)_localctx).args = explist();
					}
				}

				setState(354);
				match(CLOSEPAREN);
				}
				break;
			case 5:
				_localctx = new Exp_typecreateinstContext(_localctx);
				enterOuterAlt(_localctx, 5);
				{
				setState(356);
				match(NEW);
				setState(357);
				type_initialiser();
				}
				break;
			case 6:
				_localctx = new Exp_memberaccessContext(_localctx);
				enterOuterAlt(_localctx, 6);
				{
				setState(358);
				member_access_expression();
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 64, RULE_truth_value);
		try {
			setState(363);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case TRUE:
				enterOuterAlt(_localctx, 1);
				{
				setState(361);
				((Truth_valueContext)_localctx).value = match(TRUE);
				}
				break;
			case FALSE:
				enterOuterAlt(_localctx, 2);
				{
				setState(362);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 66, RULE_absoluteIri);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(365);
			((AbsoluteIriContext)_localctx).iri_scheme = match(IDENTIFIER);
			setState(366);
			match(COLON);
			setState(367);
			match(DIVIDE);
			setState(368);
			match(DIVIDE);
			setState(369);
			((AbsoluteIriContext)_localctx).IDENTIFIER = match(IDENTIFIER);
			((AbsoluteIriContext)_localctx).iri_domain.add(((AbsoluteIriContext)_localctx).IDENTIFIER);
			setState(374);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==DOT) {
				{
				{
				setState(370);
				match(DOT);
				setState(371);
				((AbsoluteIriContext)_localctx).IDENTIFIER = match(IDENTIFIER);
				((AbsoluteIriContext)_localctx).iri_domain.add(((AbsoluteIriContext)_localctx).IDENTIFIER);
				}
				}
				setState(376);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(381);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,31,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(377);
					match(DIVIDE);
					setState(378);
					((AbsoluteIriContext)_localctx).IDENTIFIER = match(IDENTIFIER);
					((AbsoluteIriContext)_localctx).iri_segment.add(((AbsoluteIriContext)_localctx).IDENTIFIER);
					}
					} 
				}
				setState(383);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,31,_ctx);
			}
			setState(385);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==DIVIDE) {
				{
				setState(384);
				match(DIVIDE);
				}
			}

			setState(391);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==HASH) {
				{
				setState(387);
				match(HASH);
				setState(389);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==IDENTIFIER) {
					{
					setState(388);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 68, RULE_alias);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(393);
			match(ALIAS);
			setState(394);
			((AliasContext)_localctx).name = packagename();
			setState(395);
			match(AS);
			setState(396);
			((AliasContext)_localctx).uri = absoluteIri();
			setState(397);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 70, RULE_iri);
		try {
			setState(401);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,35,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(399);
				qNameIri();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(400);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 72, RULE_iri_query_param);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(403);
			((Iri_query_paramContext)_localctx).name = match(IDENTIFIER);
			setState(404);
			match(ASSIGN);
			setState(405);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 74, RULE_qNameIri);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(408);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==IDENTIFIER) {
				{
				setState(407);
				((QNameIriContext)_localctx).prefix = match(IDENTIFIER);
				}
			}

			setState(410);
			match(COLON);
			setState(411);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 76, RULE_list);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(413);
			match(OPENBRACK);
			setState(414);
			((ListContext)_localctx).body = list_body();
			setState(415);
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

	@SuppressWarnings("CheckReturnValue")
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
	@SuppressWarnings("CheckReturnValue")
	public static class EListLiteralContext extends List_bodyContext {
		public List_literalContext list_literal() {
			return getRuleContext(List_literalContext.class,0);
		}
		public EListLiteralContext(List_bodyContext ctx) { copyFrom(ctx); }
	}
	@SuppressWarnings("CheckReturnValue")
	public static class EListComprehensionContext extends List_bodyContext {
		public List_comprehensionContext list_comprehension() {
			return getRuleContext(List_comprehensionContext.class,0);
		}
		public EListComprehensionContext(List_bodyContext ctx) { copyFrom(ctx); }
	}

	public final List_bodyContext list_body() throws RecognitionException {
		List_bodyContext _localctx = new List_bodyContext(_ctx, getState());
		enterRule(_localctx, 78, RULE_list_body);
		try {
			setState(419);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,37,_ctx) ) {
			case 1:
				_localctx = new EListLiteralContext(_localctx);
				enterOuterAlt(_localctx, 1);
				{
				setState(417);
				list_literal();
				}
				break;
			case 2:
				_localctx = new EListComprehensionContext(_localctx);
				enterOuterAlt(_localctx, 2);
				{
				setState(418);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 80, RULE_list_comp_constraint);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(421);
			exp();
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 82, RULE_list_comp_generator);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(423);
			((List_comp_generatorContext)_localctx).varname = var_name();
			setState(424);
			match(GEN);
			setState(425);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 84, RULE_list_literal);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(427);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 86, RULE_list_comprehension);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(429);
			((List_comprehensionContext)_localctx).varname = var_name();
			setState(430);
			match(BAR);
			setState(431);
			((List_comprehensionContext)_localctx).gen = list_comp_generator();
			{
			setState(432);
			match(COMMA);
			setState(433);
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

	@SuppressWarnings("CheckReturnValue")
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
		enterRule(_localctx, 88, RULE_list_type_signature);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(435);
			type_name();
			setState(436);
			match(OPENBRACK);
			setState(437);
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

	@SuppressWarnings("CheckReturnValue")
	public static class Var_nameContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(FifthParser.IDENTIFIER, 0); }
		public Var_nameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_var_name; }
	}

	public final Var_nameContext var_name() throws RecognitionException {
		Var_nameContext _localctx = new Var_nameContext(_ctx, getState());
		enterRule(_localctx, 90, RULE_var_name);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(439);
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

	public static final String _serializedATN =
		"\u0004\u0001H\u01ba\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
		"\u0002\u0007\u0002\u0002\u0003\u0007\u0003\u0002\u0004\u0007\u0004\u0002"+
		"\u0005\u0007\u0005\u0002\u0006\u0007\u0006\u0002\u0007\u0007\u0007\u0002"+
		"\b\u0007\b\u0002\t\u0007\t\u0002\n\u0007\n\u0002\u000b\u0007\u000b\u0002"+
		"\f\u0007\f\u0002\r\u0007\r\u0002\u000e\u0007\u000e\u0002\u000f\u0007\u000f"+
		"\u0002\u0010\u0007\u0010\u0002\u0011\u0007\u0011\u0002\u0012\u0007\u0012"+
		"\u0002\u0013\u0007\u0013\u0002\u0014\u0007\u0014\u0002\u0015\u0007\u0015"+
		"\u0002\u0016\u0007\u0016\u0002\u0017\u0007\u0017\u0002\u0018\u0007\u0018"+
		"\u0002\u0019\u0007\u0019\u0002\u001a\u0007\u001a\u0002\u001b\u0007\u001b"+
		"\u0002\u001c\u0007\u001c\u0002\u001d\u0007\u001d\u0002\u001e\u0007\u001e"+
		"\u0002\u001f\u0007\u001f\u0002 \u0007 \u0002!\u0007!\u0002\"\u0007\"\u0002"+
		"#\u0007#\u0002$\u0007$\u0002%\u0007%\u0002&\u0007&\u0002\'\u0007\'\u0002"+
		"(\u0007(\u0002)\u0007)\u0002*\u0007*\u0002+\u0007+\u0002,\u0007,\u0002"+
		"-\u0007-\u0001\u0000\u0001\u0000\u0001\u0000\u0001\u0000\u0001\u0000\u0001"+
		"\u0000\u0003\u0000c\b\u0000\u0001\u0001\u0005\u0001f\b\u0001\n\u0001\f"+
		"\u0001i\t\u0001\u0001\u0001\u0005\u0001l\b\u0001\n\u0001\f\u0001o\t\u0001"+
		"\u0001\u0001\u0001\u0001\u0005\u0001s\b\u0001\n\u0001\f\u0001v\t\u0001"+
		"\u0001\u0002\u0001\u0002\u0001\u0002\u0001\u0002\u0001\u0002\u0005\u0002"+
		"}\b\u0002\n\u0002\f\u0002\u0080\t\u0002\u0001\u0002\u0001\u0002\u0001"+
		"\u0003\u0001\u0003\u0001\u0003\u0005\u0003\u0087\b\u0003\n\u0003\f\u0003"+
		"\u008a\t\u0003\u0001\u0004\u0001\u0004\u0001\u0004\u0001\u0004\u0005\u0004"+
		"\u0090\b\u0004\n\u0004\f\u0004\u0093\t\u0004\u0001\u0004\u0001\u0004\u0001"+
		"\u0005\u0001\u0005\u0001\u0006\u0001\u0006\u0001\u0007\u0001\u0007\u0001"+
		"\u0007\u0001\u0007\u0001\u0007\u0005\u0007\u00a0\b\u0007\n\u0007\f\u0007"+
		"\u00a3\t\u0007\u0001\u0007\u0001\u0007\u0001\b\u0001\b\u0001\b\u0001\b"+
		"\u0001\b\u0001\t\u0001\t\u0001\t\u0001\t\u0001\t\u0005\t\u00b1\b\t\n\t"+
		"\f\t\u00b4\t\t\u0001\t\u0001\t\u0001\n\u0001\n\u0001\u000b\u0001\u000b"+
		"\u0001\u000b\u0001\u000b\u0001\f\u0001\f\u0001\f\u0001\f\u0001\f\u0005"+
		"\f\u00c3\b\f\n\f\f\f\u00c6\t\f\u0003\f\u00c8\b\f\u0001\f\u0001\f\u0001"+
		"\f\u0001\f\u0001\f\u0001\r\u0001\r\u0001\u000e\u0001\u000e\u0001\u000f"+
		"\u0001\u000f\u0001\u0010\u0001\u0010\u0001\u0010\u0001\u0011\u0001\u0011"+
		"\u0001\u0011\u0001\u0011\u0001\u0011\u0003\u0011\u00dd\b\u0011\u0001\u0012"+
		"\u0001\u0012\u0001\u0013\u0001\u0013\u0001\u0014\u0001\u0014\u0001\u0014"+
		"\u0001\u0014\u0005\u0014\u00e7\b\u0014\n\u0014\f\u0014\u00ea\t\u0014\u0001"+
		"\u0014\u0001\u0014\u0001\u0015\u0001\u0015\u0001\u0015\u0001\u0015\u0001"+
		"\u0015\u0003\u0015\u00f3\b\u0015\u0001\u0016\u0001\u0016\u0001\u0016\u0001"+
		"\u0016\u0005\u0016\u00f9\b\u0016\n\u0016\f\u0016\u00fc\t\u0016\u0001\u0016"+
		"\u0001\u0016\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017"+
		"\u0001\u0017\u0001\u0017\u0003\u0017\u0107\b\u0017\u0001\u0017\u0001\u0017"+
		"\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017"+
		"\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0003\u0017"+
		"\u0116\b\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017\u0001\u0017"+
		"\u0001\u0017\u0001\u0017\u0003\u0017\u011f\b\u0017\u0001\u0018\u0001\u0018"+
		"\u0001\u0018\u0001\u0018\u0003\u0018\u0125\b\u0018\u0001\u0019\u0001\u0019"+
		"\u0001\u0019\u0005\u0019\u012a\b\u0019\n\u0019\f\u0019\u012d\t\u0019\u0001"+
		"\u001a\u0001\u001a\u0001\u001a\u0005\u001a\u0132\b\u001a\n\u001a\f\u001a"+
		"\u0135\t\u001a\u0001\u001b\u0001\u001b\u0001\u001b\u0001\u001b\u0001\u001b"+
		"\u0001\u001b\u0001\u001b\u0001\u001b\u0001\u001b\u0001\u001b\u0001\u001b"+
		"\u0001\u001b\u0003\u001b\u0143\b\u001b\u0001\u001c\u0003\u001c\u0146\b"+
		"\u001c\u0001\u001c\u0001\u001c\u0001\u001d\u0001\u001d\u0001\u001d\u0003"+
		"\u001d\u014d\b\u001d\u0001\u001e\u0001\u001e\u0001\u001e\u0003\u001e\u0152"+
		"\b\u001e\u0001\u001f\u0001\u001f\u0001\u001f\u0001\u001f\u0001\u001f\u0001"+
		"\u001f\u0001\u001f\u0001\u001f\u0001\u001f\u0001\u001f\u0001\u001f\u0001"+
		"\u001f\u0001\u001f\u0003\u001f\u0161\b\u001f\u0001\u001f\u0001\u001f\u0001"+
		"\u001f\u0001\u001f\u0001\u001f\u0003\u001f\u0168\b\u001f\u0001 \u0001"+
		" \u0003 \u016c\b \u0001!\u0001!\u0001!\u0001!\u0001!\u0001!\u0001!\u0005"+
		"!\u0175\b!\n!\f!\u0178\t!\u0001!\u0001!\u0005!\u017c\b!\n!\f!\u017f\t"+
		"!\u0001!\u0003!\u0182\b!\u0001!\u0001!\u0003!\u0186\b!\u0003!\u0188\b"+
		"!\u0001\"\u0001\"\u0001\"\u0001\"\u0001\"\u0001\"\u0001#\u0001#\u0003"+
		"#\u0192\b#\u0001$\u0001$\u0001$\u0001$\u0001%\u0003%\u0199\b%\u0001%\u0001"+
		"%\u0001%\u0001&\u0001&\u0001&\u0001&\u0001\'\u0001\'\u0003\'\u01a4\b\'"+
		"\u0001(\u0001(\u0001)\u0001)\u0001)\u0001)\u0001*\u0001*\u0001+\u0001"+
		"+\u0001+\u0001+\u0001+\u0001+\u0001,\u0001,\u0001,\u0001,\u0001-\u0001"+
		"-\u0001-\u0000\u0000.\u0000\u0002\u0004\u0006\b\n\f\u000e\u0010\u0012"+
		"\u0014\u0016\u0018\u001a\u001c\u001e \"$&(*,.02468:<>@BDFHJLNPRTVXZ\u0000"+
		"\u0001\u0001\u0000\u001c\u001d\u01c4\u0000b\u0001\u0000\u0000\u0000\u0002"+
		"g\u0001\u0000\u0000\u0000\u0004w\u0001\u0000\u0000\u0000\u0006\u0083\u0001"+
		"\u0000\u0000\u0000\b\u008b\u0001\u0000\u0000\u0000\n\u0096\u0001\u0000"+
		"\u0000\u0000\f\u0098\u0001\u0000\u0000\u0000\u000e\u009a\u0001\u0000\u0000"+
		"\u0000\u0010\u00a6\u0001\u0000\u0000\u0000\u0012\u00ab\u0001\u0000\u0000"+
		"\u0000\u0014\u00b7\u0001\u0000\u0000\u0000\u0016\u00b9\u0001\u0000\u0000"+
		"\u0000\u0018\u00bd\u0001\u0000\u0000\u0000\u001a\u00ce\u0001\u0000\u0000"+
		"\u0000\u001c\u00d0\u0001\u0000\u0000\u0000\u001e\u00d2\u0001\u0000\u0000"+
		"\u0000 \u00d4\u0001\u0000\u0000\u0000\"\u00d7\u0001\u0000\u0000\u0000"+
		"$\u00de\u0001\u0000\u0000\u0000&\u00e0\u0001\u0000\u0000\u0000(\u00e2"+
		"\u0001\u0000\u0000\u0000*\u00ed\u0001\u0000\u0000\u0000,\u00f4\u0001\u0000"+
		"\u0000\u0000.\u011e\u0001\u0000\u0000\u00000\u0120\u0001\u0000\u0000\u0000"+
		"2\u0126\u0001\u0000\u0000\u00004\u012e\u0001\u0000\u0000\u00006\u0142"+
		"\u0001\u0000\u0000\u00008\u0145\u0001\u0000\u0000\u0000:\u0149\u0001\u0000"+
		"\u0000\u0000<\u014e\u0001\u0000\u0000\u0000>\u0167\u0001\u0000\u0000\u0000"+
		"@\u016b\u0001\u0000\u0000\u0000B\u016d\u0001\u0000\u0000\u0000D\u0189"+
		"\u0001\u0000\u0000\u0000F\u0191\u0001\u0000\u0000\u0000H\u0193\u0001\u0000"+
		"\u0000\u0000J\u0198\u0001\u0000\u0000\u0000L\u019d\u0001\u0000\u0000\u0000"+
		"N\u01a3\u0001\u0000\u0000\u0000P\u01a5\u0001\u0000\u0000\u0000R\u01a7"+
		"\u0001\u0000\u0000\u0000T\u01ab\u0001\u0000\u0000\u0000V\u01ad\u0001\u0000"+
		"\u0000\u0000X\u01b3\u0001\u0000\u0000\u0000Z\u01b7\u0001\u0000\u0000\u0000"+
		"\\c\u0003Z-\u0000]c\u0003\u0004\u0002\u0000^_\u0005@\u0000\u0000_`\u0003"+
		">\u001f\u0000`a\u0005=\u0000\u0000ac\u0001\u0000\u0000\u0000b\\\u0001"+
		"\u0000\u0000\u0000b]\u0001\u0000\u0000\u0000b^\u0001\u0000\u0000\u0000"+
		"c\u0001\u0001\u0000\u0000\u0000df\u0003\b\u0004\u0000ed\u0001\u0000\u0000"+
		"\u0000fi\u0001\u0000\u0000\u0000ge\u0001\u0000\u0000\u0000gh\u0001\u0000"+
		"\u0000\u0000hm\u0001\u0000\u0000\u0000ig\u0001\u0000\u0000\u0000jl\u0003"+
		"D\"\u0000kj\u0001\u0000\u0000\u0000lo\u0001\u0000\u0000\u0000mk\u0001"+
		"\u0000\u0000\u0000mn\u0001\u0000\u0000\u0000nt\u0001\u0000\u0000\u0000"+
		"om\u0001\u0000\u0000\u0000ps\u0003\u0018\f\u0000qs\u0003\u000e\u0007\u0000"+
		"rp\u0001\u0000\u0000\u0000rq\u0001\u0000\u0000\u0000sv\u0001\u0000\u0000"+
		"\u0000tr\u0001\u0000\u0000\u0000tu\u0001\u0000\u0000\u0000u\u0003\u0001"+
		"\u0000\u0000\u0000vt\u0001\u0000\u0000\u0000wx\u0003\u001c\u000e\u0000"+
		"xy\u0005@\u0000\u0000y~\u0003>\u001f\u0000z{\u00058\u0000\u0000{}\u0003"+
		">\u001f\u0000|z\u0001\u0000\u0000\u0000}\u0080\u0001\u0000\u0000\u0000"+
		"~|\u0001\u0000\u0000\u0000~\u007f\u0001\u0000\u0000\u0000\u007f\u0081"+
		"\u0001\u0000\u0000\u0000\u0080~\u0001\u0000\u0000\u0000\u0081\u0082\u0005"+
		"=\u0000\u0000\u0082\u0005\u0001\u0000\u0000\u0000\u0083\u0088\u0003\u0000"+
		"\u0000\u0000\u0084\u0085\u00054\u0000\u0000\u0085\u0087\u0003\u0000\u0000"+
		"\u0000\u0086\u0084\u0001\u0000\u0000\u0000\u0087\u008a\u0001\u0000\u0000"+
		"\u0000\u0088\u0086\u0001\u0000\u0000\u0000\u0088\u0089\u0001\u0000\u0000"+
		"\u0000\u0089\u0007\u0001\u0000\u0000\u0000\u008a\u0088\u0001\u0000\u0000"+
		"\u0000\u008b\u008c\u0005\r\u0000\u0000\u008c\u0091\u0003\n\u0005\u0000"+
		"\u008d\u008e\u00058\u0000\u0000\u008e\u0090\u0003\n\u0005\u0000\u008f"+
		"\u008d\u0001\u0000\u0000\u0000\u0090\u0093\u0001\u0000\u0000\u0000\u0091"+
		"\u008f\u0001\u0000\u0000\u0000\u0091\u0092\u0001\u0000\u0000\u0000\u0092"+
		"\u0094\u0001\u0000\u0000\u0000\u0093\u0091\u0001\u0000\u0000\u0000\u0094"+
		"\u0095\u00057\u0000\u0000\u0095\t\u0001\u0000\u0000\u0000\u0096\u0097"+
		"\u0005C\u0000\u0000\u0097\u000b\u0001\u0000\u0000\u0000\u0098\u0099\u0005"+
		"C\u0000\u0000\u0099\r\u0001\u0000\u0000\u0000\u009a\u009b\u0005\u0006"+
		"\u0000\u0000\u009b\u009c\u0005C\u0000\u0000\u009c\u00a1\u0005>\u0000\u0000"+
		"\u009d\u00a0\u0003\u0018\f\u0000\u009e\u00a0\u0003\u0010\b\u0000\u009f"+
		"\u009d\u0001\u0000\u0000\u0000\u009f\u009e\u0001\u0000\u0000\u0000\u00a0"+
		"\u00a3\u0001\u0000\u0000\u0000\u00a1\u009f\u0001\u0000\u0000\u0000\u00a1"+
		"\u00a2\u0001\u0000\u0000\u0000\u00a2\u00a4\u0001\u0000\u0000\u0000\u00a3"+
		"\u00a1\u0001\u0000\u0000\u0000\u00a4\u00a5\u0005;\u0000\u0000\u00a5\u000f"+
		"\u0001\u0000\u0000\u0000\u00a6\u00a7\u0005C\u0000\u0000\u00a7\u00a8\u0005"+
		"6\u0000\u0000\u00a8\u00a9\u0005C\u0000\u0000\u00a9\u00aa\u00057\u0000"+
		"\u0000\u00aa\u0011\u0001\u0000\u0000\u0000\u00ab\u00ac\u0003\u0014\n\u0000"+
		"\u00ac\u00ad\u0005>\u0000\u0000\u00ad\u00b2\u0003\u0016\u000b\u0000\u00ae"+
		"\u00af\u00058\u0000\u0000\u00af\u00b1\u0003\u0016\u000b\u0000\u00b0\u00ae"+
		"\u0001\u0000\u0000\u0000\u00b1\u00b4\u0001\u0000\u0000\u0000\u00b2\u00b0"+
		"\u0001\u0000\u0000\u0000\u00b2\u00b3\u0001\u0000\u0000\u0000\u00b3\u00b5"+
		"\u0001\u0000\u0000\u0000\u00b4\u00b2\u0001\u0000\u0000\u0000\u00b5\u00b6"+
		"\u0005;\u0000\u0000\u00b6\u0013\u0001\u0000\u0000\u0000\u00b7\u00b8\u0005"+
		"C\u0000\u0000\u00b8\u0015\u0001\u0000\u0000\u0000\u00b9\u00ba\u0003Z-"+
		"\u0000\u00ba\u00bb\u00059\u0000\u0000\u00bb\u00bc\u0003>\u001f\u0000\u00bc"+
		"\u0017\u0001\u0000\u0000\u0000\u00bd\u00be\u0003\u001c\u000e\u0000\u00be"+
		"\u00c7\u0005@\u0000\u0000\u00bf\u00c4\u0003\"\u0011\u0000\u00c0\u00c1"+
		"\u00058\u0000\u0000\u00c1\u00c3\u0003\"\u0011\u0000\u00c2\u00c0\u0001"+
		"\u0000\u0000\u0000\u00c3\u00c6\u0001\u0000\u0000\u0000\u00c4\u00c2\u0001"+
		"\u0000\u0000\u0000\u00c4\u00c5\u0001\u0000\u0000\u0000\u00c5\u00c8\u0001"+
		"\u0000\u0000\u0000\u00c6\u00c4\u0001\u0000\u0000\u0000\u00c7\u00bf\u0001"+
		"\u0000\u0000\u0000\u00c7\u00c8\u0001\u0000\u0000\u0000\u00c8\u00c9\u0001"+
		"\u0000\u0000\u0000\u00c9\u00ca\u0005=\u0000\u0000\u00ca\u00cb\u00056\u0000"+
		"\u0000\u00cb\u00cc\u0003\u001e\u000f\u0000\u00cc\u00cd\u0003\u001a\r\u0000"+
		"\u00cd\u0019\u0001\u0000\u0000\u0000\u00ce\u00cf\u0003,\u0016\u0000\u00cf"+
		"\u001b\u0001\u0000\u0000\u0000\u00d0\u00d1\u00032\u0019\u0000\u00d1\u001d"+
		"\u0001\u0000\u0000\u0000\u00d2\u00d3\u0005C\u0000\u0000\u00d3\u001f\u0001"+
		"\u0000\u0000\u0000\u00d4\u00d5\u0005:\u0000\u0000\u00d5\u00d6\u0003>\u001f"+
		"\u0000\u00d6!\u0001\u0000\u0000\u0000\u00d7\u00d8\u0003$\u0012\u0000\u00d8"+
		"\u00d9\u00056\u0000\u0000\u00d9\u00dc\u0003&\u0013\u0000\u00da\u00dd\u0003"+
		" \u0010\u0000\u00db\u00dd\u0003(\u0014\u0000\u00dc\u00da\u0001\u0000\u0000"+
		"\u0000\u00dc\u00db\u0001\u0000\u0000\u0000\u00dc\u00dd\u0001\u0000\u0000"+
		"\u0000\u00dd#\u0001\u0000\u0000\u0000\u00de\u00df\u0005C\u0000\u0000\u00df"+
		"%\u0001\u0000\u0000\u0000\u00e0\u00e1\u00032\u0019\u0000\u00e1\'\u0001"+
		"\u0000\u0000\u0000\u00e2\u00e3\u0005>\u0000\u0000\u00e3\u00e8\u0003*\u0015"+
		"\u0000\u00e4\u00e5\u00058\u0000\u0000\u00e5\u00e7\u0003*\u0015\u0000\u00e6"+
		"\u00e4\u0001\u0000\u0000\u0000\u00e7\u00ea\u0001\u0000\u0000\u0000\u00e8"+
		"\u00e6\u0001\u0000\u0000\u0000\u00e8\u00e9\u0001\u0000\u0000\u0000\u00e9"+
		"\u00eb\u0001\u0000\u0000\u0000\u00ea\u00e8\u0001\u0000\u0000\u0000\u00eb"+
		"\u00ec\u0005;\u0000\u0000\u00ec)\u0001\u0000\u0000\u0000\u00ed\u00ee\u0005"+
		"C\u0000\u0000\u00ee\u00ef\u00056\u0000\u0000\u00ef\u00f2\u0005C\u0000"+
		"\u0000\u00f0\u00f3\u0003 \u0010\u0000\u00f1\u00f3\u0003(\u0014\u0000\u00f2"+
		"\u00f0\u0001\u0000\u0000\u0000\u00f2\u00f1\u0001\u0000\u0000\u0000\u00f2"+
		"\u00f3\u0001\u0000\u0000\u0000\u00f3+\u0001\u0000\u0000\u0000\u00f4\u00fa"+
		"\u0005>\u0000\u0000\u00f5\u00f6\u0003.\u0017\u0000\u00f6\u00f7\u00057"+
		"\u0000\u0000\u00f7\u00f9\u0001\u0000\u0000\u0000\u00f8\u00f5\u0001\u0000"+
		"\u0000\u0000\u00f9\u00fc\u0001\u0000\u0000\u0000\u00fa\u00f8\u0001\u0000"+
		"\u0000\u0000\u00fa\u00fb\u0001\u0000\u0000\u0000\u00fb\u00fd\u0001\u0000"+
		"\u0000\u0000\u00fc\u00fa\u0001\u0000\u0000\u0000\u00fd\u00fe\u0005;\u0000"+
		"\u0000\u00fe-\u0001\u0000\u0000\u0000\u00ff\u0100\u0005\t\u0000\u0000"+
		"\u0100\u0101\u0005@\u0000\u0000\u0101\u0102\u0003>\u001f\u0000\u0102\u0103"+
		"\u0005=\u0000\u0000\u0103\u0106\u0003,\u0016\u0000\u0104\u0105\u0005\u0007"+
		"\u0000\u0000\u0105\u0107\u0003,\u0016\u0000\u0106\u0104\u0001\u0000\u0000"+
		"\u0000\u0106\u0107\u0001\u0000\u0000\u0000\u0107\u011f\u0001\u0000\u0000"+
		"\u0000\u0108\u0109\u0005\u000f\u0000\u0000\u0109\u010a\u0005@\u0000\u0000"+
		"\u010a\u010b\u0003>\u001f\u0000\u010b\u010c\u0005=\u0000\u0000\u010c\u010d"+
		"\u0003,\u0016\u0000\u010d\u011f\u0001\u0000\u0000\u0000\u010e\u010f\u0005"+
		"\u0010\u0000\u0000\u010f\u0110\u0003>\u001f\u0000\u0110\u0111\u0003,\u0016"+
		"\u0000\u0111\u011f\u0001\u0000\u0000\u0000\u0112\u0115\u00030\u0018\u0000"+
		"\u0113\u0114\u00059\u0000\u0000\u0114\u0116\u0003>\u001f\u0000\u0115\u0113"+
		"\u0001\u0000\u0000\u0000\u0115\u0116\u0001\u0000\u0000\u0000\u0116\u011f"+
		"\u0001\u0000\u0000\u0000\u0117\u0118\u0003Z-\u0000\u0118\u0119\u00059"+
		"\u0000\u0000\u0119\u011a\u0003>\u001f\u0000\u011a\u011f\u0001\u0000\u0000"+
		"\u0000\u011b\u011c\u0005\f\u0000\u0000\u011c\u011f\u0003>\u001f\u0000"+
		"\u011d\u011f\u0003>\u001f\u0000\u011e\u00ff\u0001\u0000\u0000\u0000\u011e"+
		"\u0108\u0001\u0000\u0000\u0000\u011e\u010e\u0001\u0000\u0000\u0000\u011e"+
		"\u0112\u0001\u0000\u0000\u0000\u011e\u0117\u0001\u0000\u0000\u0000\u011e"+
		"\u011b\u0001\u0000\u0000\u0000\u011e\u011d\u0001\u0000\u0000\u0000\u011f"+
		"/\u0001\u0000\u0000\u0000\u0120\u0121\u0003Z-\u0000\u0121\u0124\u0005"+
		"6\u0000\u0000\u0122\u0125\u0003\u0014\n\u0000\u0123\u0125\u0003X,\u0000"+
		"\u0124\u0122\u0001\u0000\u0000\u0000\u0124\u0123\u0001\u0000\u0000\u0000"+
		"\u01251\u0001\u0000\u0000\u0000\u0126\u012b\u0005C\u0000\u0000\u0127\u0128"+
		"\u00054\u0000\u0000\u0128\u012a\u0005C\u0000\u0000\u0129\u0127\u0001\u0000"+
		"\u0000\u0000\u012a\u012d\u0001\u0000\u0000\u0000\u012b\u0129\u0001\u0000"+
		"\u0000\u0000\u012b\u012c\u0001\u0000\u0000\u0000\u012c3\u0001\u0000\u0000"+
		"\u0000\u012d\u012b\u0001\u0000\u0000\u0000\u012e\u0133\u0003>\u001f\u0000"+
		"\u012f\u0130\u00058\u0000\u0000\u0130\u0132\u0003>\u001f\u0000\u0131\u012f"+
		"\u0001\u0000\u0000\u0000\u0132\u0135\u0001\u0000\u0000\u0000\u0133\u0131"+
		"\u0001\u0000\u0000\u0000\u0133\u0134\u0001\u0000\u0000\u0000\u01345\u0001"+
		"\u0000\u0000\u0000\u0135\u0133\u0001\u0000\u0000\u0000\u0136\u0143\u0003"+
		"@ \u0000\u0137\u0138\u0005E\u0000\u0000\u0138\u0143\u0005\u0001\u0000"+
		"\u0000\u0139\u0143\u0005E\u0000\u0000\u013a\u013b\u0005E\u0000\u0000\u013b"+
		"\u0143\u0005\u0002\u0000\u0000\u013c\u0143\u0005F\u0000\u0000\u013d\u013e"+
		"\u0005F\u0000\u0000\u013e\u0143\u0005\u0003\u0000\u0000\u013f\u0143\u0005"+
		"D\u0000\u0000\u0140\u0143\u0003L&\u0000\u0141\u0143\u0003Z-\u0000\u0142"+
		"\u0136\u0001\u0000\u0000\u0000\u0142\u0137\u0001\u0000\u0000\u0000\u0142"+
		"\u0139\u0001\u0000\u0000\u0000\u0142\u013a\u0001\u0000\u0000\u0000\u0142"+
		"\u013c\u0001\u0000\u0000\u0000\u0142\u013d\u0001\u0000\u0000\u0000\u0142"+
		"\u013f\u0001\u0000\u0000\u0000\u0142\u0140\u0001\u0000\u0000\u0000\u0142"+
		"\u0141\u0001\u0000\u0000\u0000\u01437\u0001\u0000\u0000\u0000\u0144\u0146"+
		"\u00050\u0000\u0000\u0145\u0144\u0001\u0000\u0000\u0000\u0145\u0146\u0001"+
		"\u0000\u0000\u0000\u0146\u0147\u0001\u0000\u0000\u0000\u0147\u0148\u0003"+
		"6\u001b\u0000\u01489\u0001\u0000\u0000\u0000\u0149\u014c\u00038\u001c"+
		"\u0000\u014a\u014b\u0007\u0000\u0000\u0000\u014b\u014d\u00038\u001c\u0000"+
		"\u014c\u014a\u0001\u0000\u0000\u0000\u014c\u014d\u0001\u0000\u0000\u0000"+
		"\u014d;\u0001\u0000\u0000\u0000\u014e\u0151\u0003:\u001d\u0000\u014f\u0150"+
		"\u00051\u0000\u0000\u0150\u0152\u0003:\u001d\u0000\u0151\u014f\u0001\u0000"+
		"\u0000\u0000\u0151\u0152\u0001\u0000\u0000\u0000\u0152=\u0001\u0000\u0000"+
		"\u0000\u0153\u0154\u0005@\u0000\u0000\u0154\u0155\u0003>\u001f\u0000\u0155"+
		"\u0156\u0005=\u0000\u0000\u0156\u0168\u0001\u0000\u0000\u0000\u0157\u0158"+
		"\u0005@\u0000\u0000\u0158\u0159\u0003\u0014\n\u0000\u0159\u015a\u0005"+
		"=\u0000\u0000\u015a\u015b\u0003>\u001f\u0000\u015b\u0168\u0001\u0000\u0000"+
		"\u0000\u015c\u0168\u0003<\u001e\u0000\u015d\u015e\u0003\u001c\u000e\u0000"+
		"\u015e\u0160\u0005@\u0000\u0000\u015f\u0161\u00034\u001a\u0000\u0160\u015f"+
		"\u0001\u0000\u0000\u0000\u0160\u0161\u0001\u0000\u0000\u0000\u0161\u0162"+
		"\u0001\u0000\u0000\u0000\u0162\u0163\u0005=\u0000\u0000\u0163\u0168\u0001"+
		"\u0000\u0000\u0000\u0164\u0165\u0005\u000b\u0000\u0000\u0165\u0168\u0003"+
		"\u0012\t\u0000\u0166\u0168\u0003\u0006\u0003\u0000\u0167\u0153\u0001\u0000"+
		"\u0000\u0000\u0167\u0157\u0001\u0000\u0000\u0000\u0167\u015c\u0001\u0000"+
		"\u0000\u0000\u0167\u015d\u0001\u0000\u0000\u0000\u0167\u0164\u0001\u0000"+
		"\u0000\u0000\u0167\u0166\u0001\u0000\u0000\u0000\u0168?\u0001\u0000\u0000"+
		"\u0000\u0169\u016c\u0005\u000e\u0000\u0000\u016a\u016c\u0005\b\u0000\u0000"+
		"\u016b\u0169\u0001\u0000\u0000\u0000\u016b\u016a\u0001\u0000\u0000\u0000"+
		"\u016cA\u0001\u0000\u0000\u0000\u016d\u016e\u0005C\u0000\u0000\u016e\u016f"+
		"\u00056\u0000\u0000\u016f\u0170\u0005H\u0000\u0000\u0170\u0171\u0005H"+
		"\u0000\u0000\u0171\u0176\u0005C\u0000\u0000\u0172\u0173\u00054\u0000\u0000"+
		"\u0173\u0175\u0005C\u0000\u0000\u0174\u0172\u0001\u0000\u0000\u0000\u0175"+
		"\u0178\u0001\u0000\u0000\u0000\u0176\u0174\u0001\u0000\u0000\u0000\u0176"+
		"\u0177\u0001\u0000\u0000\u0000\u0177\u017d\u0001\u0000\u0000\u0000\u0178"+
		"\u0176\u0001\u0000\u0000\u0000\u0179\u017a\u0005H\u0000\u0000\u017a\u017c"+
		"\u0005C\u0000\u0000\u017b\u0179\u0001\u0000\u0000\u0000\u017c\u017f\u0001"+
		"\u0000\u0000\u0000\u017d\u017b\u0001\u0000\u0000\u0000\u017d\u017e\u0001"+
		"\u0000\u0000\u0000\u017e\u0181\u0001\u0000\u0000\u0000\u017f\u017d\u0001"+
		"\u0000\u0000\u0000\u0180\u0182\u0005H\u0000\u0000\u0181\u0180\u0001\u0000"+
		"\u0000\u0000\u0181\u0182\u0001\u0000\u0000\u0000\u0182\u0187\u0001\u0000"+
		"\u0000\u0000\u0183\u0185\u00053\u0000\u0000\u0184\u0186\u0005C\u0000\u0000"+
		"\u0185\u0184\u0001\u0000\u0000\u0000\u0185\u0186\u0001\u0000\u0000\u0000"+
		"\u0186\u0188\u0001\u0000\u0000\u0000\u0187\u0183\u0001\u0000\u0000\u0000"+
		"\u0187\u0188\u0001\u0000\u0000\u0000\u0188C\u0001\u0000\u0000\u0000\u0189"+
		"\u018a\u0005\u0004\u0000\u0000\u018a\u018b\u0003\f\u0006\u0000\u018b\u018c"+
		"\u0005\u0005\u0000\u0000\u018c\u018d\u0003B!\u0000\u018d\u018e\u00057"+
		"\u0000\u0000\u018eE\u0001\u0000\u0000\u0000\u018f\u0192\u0003J%\u0000"+
		"\u0190\u0192\u0003B!\u0000\u0191\u018f\u0001\u0000\u0000\u0000\u0191\u0190"+
		"\u0001\u0000\u0000\u0000\u0192G\u0001\u0000\u0000\u0000\u0193\u0194\u0005"+
		"C\u0000\u0000\u0194\u0195\u00059\u0000\u0000\u0195\u0196\u0005C\u0000"+
		"\u0000\u0196I\u0001\u0000\u0000\u0000\u0197\u0199\u0005C\u0000\u0000\u0198"+
		"\u0197\u0001\u0000\u0000\u0000\u0198\u0199\u0001\u0000\u0000\u0000\u0199"+
		"\u019a\u0001\u0000\u0000\u0000\u019a\u019b\u00056\u0000\u0000\u019b\u019c"+
		"\u0005C\u0000\u0000\u019cK\u0001\u0000\u0000\u0000\u019d\u019e\u0005?"+
		"\u0000\u0000\u019e\u019f\u0003N\'\u0000\u019f\u01a0\u0005<\u0000\u0000"+
		"\u01a0M\u0001\u0000\u0000\u0000\u01a1\u01a4\u0003T*\u0000\u01a2\u01a4"+
		"\u0003V+\u0000\u01a3\u01a1\u0001\u0000\u0000\u0000\u01a3\u01a2\u0001\u0000"+
		"\u0000\u0000\u01a4O\u0001\u0000\u0000\u0000\u01a5\u01a6\u0003>\u001f\u0000"+
		"\u01a6Q\u0001\u0000\u0000\u0000\u01a7\u01a8\u0003Z-\u0000\u01a8\u01a9"+
		"\u00055\u0000\u0000\u01a9\u01aa\u0003Z-\u0000\u01aaS\u0001\u0000\u0000"+
		"\u0000\u01ab\u01ac\u00034\u001a\u0000\u01acU\u0001\u0000\u0000\u0000\u01ad"+
		"\u01ae\u0003Z-\u0000\u01ae\u01af\u0005:\u0000\u0000\u01af\u01b0\u0003"+
		"R)\u0000\u01b0\u01b1\u00058\u0000\u0000\u01b1\u01b2\u0003P(\u0000\u01b2"+
		"W\u0001\u0000\u0000\u0000\u01b3\u01b4\u0003\u0014\n\u0000\u01b4\u01b5"+
		"\u0005?\u0000\u0000\u01b5\u01b6\u0005<\u0000\u0000\u01b6Y\u0001\u0000"+
		"\u0000\u0000\u01b7\u01b8\u0005C\u0000\u0000\u01b8[\u0001\u0000\u0000\u0000"+
		"&bgmrt~\u0088\u0091\u009f\u00a1\u00b2\u00c4\u00c7\u00dc\u00e8\u00f2\u00fa"+
		"\u0106\u0115\u011e\u0124\u012b\u0133\u0142\u0145\u014c\u0151\u0160\u0167"+
		"\u016b\u0176\u017d\u0181\u0185\u0187\u0191\u0198\u01a3";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}