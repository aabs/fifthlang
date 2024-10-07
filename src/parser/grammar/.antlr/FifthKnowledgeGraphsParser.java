// Generated from c:\dev\fifthlang\fifth.parser\grammar\FifthKnowledgeGraphs.g4 by ANTLR 4.8
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class FifthKnowledgeGraphsParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.8", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		IDENTIFIER=1, COLON=2, DIVIDE=3, DOT=4, HASH=5, ASSIGN=6;
	public static final int
		RULE_iri = 0, RULE_qNameIri = 1, RULE_absoluteIri = 2, RULE_iri_query_param = 3;
	private static String[] makeRuleNames() {
		return new String[] {
			"iri", "qNameIri", "absoluteIri", "iri_query_param"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "IDENTIFIER", "COLON", "DIVIDE", "DOT", "HASH", "ASSIGN"
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
	public String getGrammarFileName() { return "FifthKnowledgeGraphs.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public FifthKnowledgeGraphsParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
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
		enterRule(_localctx, 0, RULE_iri);
		try {
			setState(10);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,0,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(8);
				qNameIri();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(9);
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
		public TerminalNode COLON() { return getToken(FifthKnowledgeGraphsParser.COLON, 0); }
		public List<TerminalNode> IDENTIFIER() { return getTokens(FifthKnowledgeGraphsParser.IDENTIFIER); }
		public TerminalNode IDENTIFIER(int i) {
			return getToken(FifthKnowledgeGraphsParser.IDENTIFIER, i);
		}
		public QNameIriContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_qNameIri; }
	}

	public final QNameIriContext qNameIri() throws RecognitionException {
		QNameIriContext _localctx = new QNameIriContext(_ctx, getState());
		enterRule(_localctx, 2, RULE_qNameIri);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(13);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==IDENTIFIER) {
				{
				setState(12);
				((QNameIriContext)_localctx).prefix = match(IDENTIFIER);
				}
			}

			setState(15);
			match(COLON);
			setState(16);
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
		public TerminalNode COLON() { return getToken(FifthKnowledgeGraphsParser.COLON, 0); }
		public List<TerminalNode> DIVIDE() { return getTokens(FifthKnowledgeGraphsParser.DIVIDE); }
		public TerminalNode DIVIDE(int i) {
			return getToken(FifthKnowledgeGraphsParser.DIVIDE, i);
		}
		public List<TerminalNode> IDENTIFIER() { return getTokens(FifthKnowledgeGraphsParser.IDENTIFIER); }
		public TerminalNode IDENTIFIER(int i) {
			return getToken(FifthKnowledgeGraphsParser.IDENTIFIER, i);
		}
		public List<TerminalNode> DOT() { return getTokens(FifthKnowledgeGraphsParser.DOT); }
		public TerminalNode DOT(int i) {
			return getToken(FifthKnowledgeGraphsParser.DOT, i);
		}
		public TerminalNode HASH() { return getToken(FifthKnowledgeGraphsParser.HASH, 0); }
		public AbsoluteIriContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_absoluteIri; }
	}

	public final AbsoluteIriContext absoluteIri() throws RecognitionException {
		AbsoluteIriContext _localctx = new AbsoluteIriContext(_ctx, getState());
		enterRule(_localctx, 4, RULE_absoluteIri);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(18);
			((AbsoluteIriContext)_localctx).iri_scheme = match(IDENTIFIER);
			setState(19);
			match(COLON);
			setState(20);
			match(DIVIDE);
			setState(21);
			match(DIVIDE);
			setState(22);
			((AbsoluteIriContext)_localctx).IDENTIFIER = match(IDENTIFIER);
			((AbsoluteIriContext)_localctx).iri_domain.add(((AbsoluteIriContext)_localctx).IDENTIFIER);
			setState(27);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==DOT) {
				{
				{
				setState(23);
				match(DOT);
				setState(24);
				((AbsoluteIriContext)_localctx).IDENTIFIER = match(IDENTIFIER);
				((AbsoluteIriContext)_localctx).iri_domain.add(((AbsoluteIriContext)_localctx).IDENTIFIER);
				}
				}
				setState(29);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(34);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,3,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					setState(30);
					match(DIVIDE);
					setState(31);
					((AbsoluteIriContext)_localctx).IDENTIFIER = match(IDENTIFIER);
					((AbsoluteIriContext)_localctx).iri_segment.add(((AbsoluteIriContext)_localctx).IDENTIFIER);
					}
					} 
				}
				setState(36);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,3,_ctx);
			}
			setState(38);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==DIVIDE) {
				{
				setState(37);
				match(DIVIDE);
				}
			}

			setState(44);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==HASH) {
				{
				setState(40);
				match(HASH);
				setState(42);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==IDENTIFIER) {
					{
					setState(41);
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
		public TerminalNode ASSIGN() { return getToken(FifthKnowledgeGraphsParser.ASSIGN, 0); }
		public List<TerminalNode> IDENTIFIER() { return getTokens(FifthKnowledgeGraphsParser.IDENTIFIER); }
		public TerminalNode IDENTIFIER(int i) {
			return getToken(FifthKnowledgeGraphsParser.IDENTIFIER, i);
		}
		public Iri_query_paramContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_iri_query_param; }
	}

	public final Iri_query_paramContext iri_query_param() throws RecognitionException {
		Iri_query_paramContext _localctx = new Iri_query_paramContext(_ctx, getState());
		enterRule(_localctx, 6, RULE_iri_query_param);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(46);
			((Iri_query_paramContext)_localctx).name = match(IDENTIFIER);
			setState(47);
			match(ASSIGN);
			setState(48);
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

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\3\b\65\4\2\t\2\4\3"+
		"\t\3\4\4\t\4\4\5\t\5\3\2\3\2\5\2\r\n\2\3\3\5\3\20\n\3\3\3\3\3\3\3\3\4"+
		"\3\4\3\4\3\4\3\4\3\4\3\4\7\4\34\n\4\f\4\16\4\37\13\4\3\4\3\4\7\4#\n\4"+
		"\f\4\16\4&\13\4\3\4\5\4)\n\4\3\4\3\4\5\4-\n\4\5\4/\n\4\3\5\3\5\3\5\3\5"+
		"\3\5\2\2\6\2\4\6\b\2\2\2\67\2\f\3\2\2\2\4\17\3\2\2\2\6\24\3\2\2\2\b\60"+
		"\3\2\2\2\n\r\5\4\3\2\13\r\5\6\4\2\f\n\3\2\2\2\f\13\3\2\2\2\r\3\3\2\2\2"+
		"\16\20\7\3\2\2\17\16\3\2\2\2\17\20\3\2\2\2\20\21\3\2\2\2\21\22\7\4\2\2"+
		"\22\23\7\3\2\2\23\5\3\2\2\2\24\25\7\3\2\2\25\26\7\4\2\2\26\27\7\5\2\2"+
		"\27\30\7\5\2\2\30\35\7\3\2\2\31\32\7\6\2\2\32\34\7\3\2\2\33\31\3\2\2\2"+
		"\34\37\3\2\2\2\35\33\3\2\2\2\35\36\3\2\2\2\36$\3\2\2\2\37\35\3\2\2\2 "+
		"!\7\5\2\2!#\7\3\2\2\" \3\2\2\2#&\3\2\2\2$\"\3\2\2\2$%\3\2\2\2%(\3\2\2"+
		"\2&$\3\2\2\2\')\7\5\2\2(\'\3\2\2\2()\3\2\2\2).\3\2\2\2*,\7\7\2\2+-\7\3"+
		"\2\2,+\3\2\2\2,-\3\2\2\2-/\3\2\2\2.*\3\2\2\2./\3\2\2\2/\7\3\2\2\2\60\61"+
		"\7\3\2\2\61\62\7\b\2\2\62\63\7\3\2\2\63\t\3\2\2\2\t\f\17\35$(,.";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}