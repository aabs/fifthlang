// Generated from c:\dev\fifthlang\fifth.parser\grammar\FifthLexicalRules.g4 by ANTLR 4.8
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class FifthLexicalRules extends Lexer {
	static { RuntimeMetaData.checkVersion("4.8", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		AMP=1, AND=2, ASSIGN=3, BAR=4, CLOSEBRACE=5, CLOSEBRACK=6, CLOSEPAREN=7, 
		COLON=8, COMMA=9, DIVIDE=10, DOT=11, EQ=12, GEN=13, GEQ=14, GT=15, HASH=16, 
		LAMBDASEP=17, LEQ=18, LT=19, MINUS=20, NEQ=21, NOT=22, OPENBRACE=23, OPENBRACK=24, 
		OPENPAREN=25, OR=26, PERCENT=27, PLUS=28, POWER=29, QMARK=30, SEMICOLON=31, 
		TIMES=32, IDENTIFIER=33, STRING=34, INT=35, FLOAT=36, WS=37;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"AMP", "AND", "ASSIGN", "BAR", "CLOSEBRACE", "CLOSEBRACK", "CLOSEPAREN", 
			"COLON", "COMMA", "DIVIDE", "DOT", "EQ", "GEN", "GEQ", "GT", "HASH", 
			"LAMBDASEP", "LEQ", "LT", "MINUS", "NEQ", "NOT", "OPENBRACE", "OPENBRACK", 
			"OPENPAREN", "OR", "PERCENT", "PLUS", "POWER", "QMARK", "SEMICOLON", 
			"TIMES", "IDENTIFIER", "LETTER", "STRING", "EXP", "DIGIT", "HEXDIGIT", 
			"POSITIVEDIGIT", "INT", "FLOAT", "WS"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'&'", "'&&'", "'='", "'|'", "'}'", "']'", "')'", "':'", "','", 
			"'/'", "'.'", "'=='", "'<-'", "'>='", "'>'", "'#'", "'=>'", "'<='", "'<'", 
			"'-'", "'!='", "'!'", "'{'", "'['", "'('", "'||'", "'%'", "'+'", "'^'", 
			"'?'", "';'", "'*'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "AMP", "AND", "ASSIGN", "BAR", "CLOSEBRACE", "CLOSEBRACK", "CLOSEPAREN", 
			"COLON", "COMMA", "DIVIDE", "DOT", "EQ", "GEN", "GEQ", "GT", "HASH", 
			"LAMBDASEP", "LEQ", "LT", "MINUS", "NEQ", "NOT", "OPENBRACE", "OPENBRACK", 
			"OPENPAREN", "OR", "PERCENT", "PLUS", "POWER", "QMARK", "SEMICOLON", 
			"TIMES", "IDENTIFIER", "STRING", "INT", "FLOAT", "WS"
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


	public FifthLexicalRules(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "FifthLexicalRules.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public String[] getChannelNames() { return channelNames; }

	@Override
	public String[] getModeNames() { return modeNames; }

	@Override
	public ATN getATN() { return _ATN; }

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2\'\u00ec\b\1\4\2\t"+
		"\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13"+
		"\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22\t\22"+
		"\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31\t\31"+
		"\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t \4!"+
		"\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\4)\t)\4*\t*\4+\t+\3"+
		"\2\3\2\3\3\3\3\3\3\3\4\3\4\3\5\3\5\3\6\3\6\3\7\3\7\3\b\3\b\3\t\3\t\3\n"+
		"\3\n\3\13\3\13\3\f\3\f\3\r\3\r\3\r\3\16\3\16\3\16\3\17\3\17\3\17\3\20"+
		"\3\20\3\21\3\21\3\22\3\22\3\22\3\23\3\23\3\23\3\24\3\24\3\25\3\25\3\26"+
		"\3\26\3\26\3\27\3\27\3\30\3\30\3\31\3\31\3\32\3\32\3\33\3\33\3\33\3\34"+
		"\3\34\3\35\3\35\3\36\3\36\3\37\3\37\3 \3 \3!\3!\3\"\3\"\5\"\u00a2\n\""+
		"\3\"\3\"\3\"\7\"\u00a7\n\"\f\"\16\"\u00aa\13\"\3#\3#\3$\3$\7$\u00b0\n"+
		"$\f$\16$\u00b3\13$\3$\3$\3$\7$\u00b8\n$\f$\16$\u00bb\13$\3$\5$\u00be\n"+
		"$\3%\3%\5%\u00c2\n%\3%\3%\3&\3&\3\'\3\'\3(\3(\3)\3)\5)\u00ce\n)\3)\3)"+
		"\7)\u00d2\n)\f)\16)\u00d5\13)\5)\u00d7\n)\3*\5*\u00da\n*\3*\3*\3*\6*\u00df"+
		"\n*\r*\16*\u00e0\3*\5*\u00e4\n*\3+\6+\u00e7\n+\r+\16+\u00e8\3+\3+\2\2"+
		",\3\3\5\4\7\5\t\6\13\7\r\b\17\t\21\n\23\13\25\f\27\r\31\16\33\17\35\20"+
		"\37\21!\22#\23%\24\'\25)\26+\27-\30/\31\61\32\63\33\65\34\67\359\36;\37"+
		"= ?!A\"C#E\2G$I\2K\2M\2O\2Q%S&U\'\3\2\13\4\2C\\c|\3\2$$\3\2))\4\2GGgg"+
		"\4\2--//\3\2\62;\5\2\62;CHch\3\2\63;\5\2\13\f\17\17\"\"\2\u00f5\2\3\3"+
		"\2\2\2\2\5\3\2\2\2\2\7\3\2\2\2\2\t\3\2\2\2\2\13\3\2\2\2\2\r\3\2\2\2\2"+
		"\17\3\2\2\2\2\21\3\2\2\2\2\23\3\2\2\2\2\25\3\2\2\2\2\27\3\2\2\2\2\31\3"+
		"\2\2\2\2\33\3\2\2\2\2\35\3\2\2\2\2\37\3\2\2\2\2!\3\2\2\2\2#\3\2\2\2\2"+
		"%\3\2\2\2\2\'\3\2\2\2\2)\3\2\2\2\2+\3\2\2\2\2-\3\2\2\2\2/\3\2\2\2\2\61"+
		"\3\2\2\2\2\63\3\2\2\2\2\65\3\2\2\2\2\67\3\2\2\2\29\3\2\2\2\2;\3\2\2\2"+
		"\2=\3\2\2\2\2?\3\2\2\2\2A\3\2\2\2\2C\3\2\2\2\2G\3\2\2\2\2Q\3\2\2\2\2S"+
		"\3\2\2\2\2U\3\2\2\2\3W\3\2\2\2\5Y\3\2\2\2\7\\\3\2\2\2\t^\3\2\2\2\13`\3"+
		"\2\2\2\rb\3\2\2\2\17d\3\2\2\2\21f\3\2\2\2\23h\3\2\2\2\25j\3\2\2\2\27l"+
		"\3\2\2\2\31n\3\2\2\2\33q\3\2\2\2\35t\3\2\2\2\37w\3\2\2\2!y\3\2\2\2#{\3"+
		"\2\2\2%~\3\2\2\2\'\u0081\3\2\2\2)\u0083\3\2\2\2+\u0085\3\2\2\2-\u0088"+
		"\3\2\2\2/\u008a\3\2\2\2\61\u008c\3\2\2\2\63\u008e\3\2\2\2\65\u0090\3\2"+
		"\2\2\67\u0093\3\2\2\29\u0095\3\2\2\2;\u0097\3\2\2\2=\u0099\3\2\2\2?\u009b"+
		"\3\2\2\2A\u009d\3\2\2\2C\u00a1\3\2\2\2E\u00ab\3\2\2\2G\u00bd\3\2\2\2I"+
		"\u00bf\3\2\2\2K\u00c5\3\2\2\2M\u00c7\3\2\2\2O\u00c9\3\2\2\2Q\u00d6\3\2"+
		"\2\2S\u00d9\3\2\2\2U\u00e6\3\2\2\2WX\7(\2\2X\4\3\2\2\2YZ\7(\2\2Z[\7(\2"+
		"\2[\6\3\2\2\2\\]\7?\2\2]\b\3\2\2\2^_\7~\2\2_\n\3\2\2\2`a\7\177\2\2a\f"+
		"\3\2\2\2bc\7_\2\2c\16\3\2\2\2de\7+\2\2e\20\3\2\2\2fg\7<\2\2g\22\3\2\2"+
		"\2hi\7.\2\2i\24\3\2\2\2jk\7\61\2\2k\26\3\2\2\2lm\7\60\2\2m\30\3\2\2\2"+
		"no\7?\2\2op\7?\2\2p\32\3\2\2\2qr\7>\2\2rs\7/\2\2s\34\3\2\2\2tu\7@\2\2"+
		"uv\7?\2\2v\36\3\2\2\2wx\7@\2\2x \3\2\2\2yz\7%\2\2z\"\3\2\2\2{|\7?\2\2"+
		"|}\7@\2\2}$\3\2\2\2~\177\7>\2\2\177\u0080\7?\2\2\u0080&\3\2\2\2\u0081"+
		"\u0082\7>\2\2\u0082(\3\2\2\2\u0083\u0084\7/\2\2\u0084*\3\2\2\2\u0085\u0086"+
		"\7#\2\2\u0086\u0087\7?\2\2\u0087,\3\2\2\2\u0088\u0089\7#\2\2\u0089.\3"+
		"\2\2\2\u008a\u008b\7}\2\2\u008b\60\3\2\2\2\u008c\u008d\7]\2\2\u008d\62"+
		"\3\2\2\2\u008e\u008f\7*\2\2\u008f\64\3\2\2\2\u0090\u0091\7~\2\2\u0091"+
		"\u0092\7~\2\2\u0092\66\3\2\2\2\u0093\u0094\7\'\2\2\u00948\3\2\2\2\u0095"+
		"\u0096\7-\2\2\u0096:\3\2\2\2\u0097\u0098\7`\2\2\u0098<\3\2\2\2\u0099\u009a"+
		"\7A\2\2\u009a>\3\2\2\2\u009b\u009c\7=\2\2\u009c@\3\2\2\2\u009d\u009e\7"+
		",\2\2\u009eB\3\2\2\2\u009f\u00a2\5E#\2\u00a0\u00a2\7a\2\2\u00a1\u009f"+
		"\3\2\2\2\u00a1\u00a0\3\2\2\2\u00a2\u00a8\3\2\2\2\u00a3\u00a7\5E#\2\u00a4"+
		"\u00a7\5K&\2\u00a5\u00a7\7\60\2\2\u00a6\u00a3\3\2\2\2\u00a6\u00a4\3\2"+
		"\2\2\u00a6\u00a5\3\2\2\2\u00a7\u00aa\3\2\2\2\u00a8\u00a6\3\2\2\2\u00a8"+
		"\u00a9\3\2\2\2\u00a9D\3\2\2\2\u00aa\u00a8\3\2\2\2\u00ab\u00ac\t\2\2\2"+
		"\u00acF\3\2\2\2\u00ad\u00b1\7$\2\2\u00ae\u00b0\n\3\2\2\u00af\u00ae\3\2"+
		"\2\2\u00b0\u00b3\3\2\2\2\u00b1\u00af\3\2\2\2\u00b1\u00b2\3\2\2\2\u00b2"+
		"\u00b4\3\2\2\2\u00b3\u00b1\3\2\2\2\u00b4\u00be\7$\2\2\u00b5\u00b9\7)\2"+
		"\2\u00b6\u00b8\n\4\2\2\u00b7\u00b6\3\2\2\2\u00b8\u00bb\3\2\2\2\u00b9\u00b7"+
		"\3\2\2\2\u00b9\u00ba\3\2\2\2\u00ba\u00bc\3\2\2\2\u00bb\u00b9\3\2\2\2\u00bc"+
		"\u00be\7)\2\2\u00bd\u00ad\3\2\2\2\u00bd\u00b5\3\2\2\2\u00beH\3\2\2\2\u00bf"+
		"\u00c1\t\5\2\2\u00c0\u00c2\t\6\2\2\u00c1\u00c0\3\2\2\2\u00c1\u00c2\3\2"+
		"\2\2\u00c2\u00c3\3\2\2\2\u00c3\u00c4\5Q)\2\u00c4J\3\2\2\2\u00c5\u00c6"+
		"\t\7\2\2\u00c6L\3\2\2\2\u00c7\u00c8\t\b\2\2\u00c8N\3\2\2\2\u00c9\u00ca"+
		"\t\t\2\2\u00caP\3\2\2\2\u00cb\u00d7\7\62\2\2\u00cc\u00ce\7/\2\2\u00cd"+
		"\u00cc\3\2\2\2\u00cd\u00ce\3\2\2\2\u00ce\u00cf\3\2\2\2\u00cf\u00d3\5O"+
		"(\2\u00d0\u00d2\5K&\2\u00d1\u00d0\3\2\2\2\u00d2\u00d5\3\2\2\2\u00d3\u00d1"+
		"\3\2\2\2\u00d3\u00d4\3\2\2\2\u00d4\u00d7\3\2\2\2\u00d5\u00d3\3\2\2\2\u00d6"+
		"\u00cb\3\2\2\2\u00d6\u00cd\3\2\2\2\u00d7R\3\2\2\2\u00d8\u00da\7/\2\2\u00d9"+
		"\u00d8\3\2\2\2\u00d9\u00da\3\2\2\2\u00da\u00db\3\2\2\2\u00db\u00dc\5Q"+
		")\2\u00dc\u00de\7\60\2\2\u00dd\u00df\5K&\2\u00de\u00dd\3\2\2\2\u00df\u00e0"+
		"\3\2\2\2\u00e0\u00de\3\2\2\2\u00e0\u00e1\3\2\2\2\u00e1\u00e3\3\2\2\2\u00e2"+
		"\u00e4\5I%\2\u00e3\u00e2\3\2\2\2\u00e3\u00e4\3\2\2\2\u00e4T\3\2\2\2\u00e5"+
		"\u00e7\t\n\2\2\u00e6\u00e5\3\2\2\2\u00e7\u00e8\3\2\2\2\u00e8\u00e6\3\2"+
		"\2\2\u00e8\u00e9\3\2\2\2\u00e9\u00ea\3\2\2\2\u00ea\u00eb\b+\2\2\u00eb"+
		"V\3\2\2\2\21\2\u00a1\u00a6\u00a8\u00b1\u00b9\u00bd\u00c1\u00cd\u00d3\u00d6"+
		"\u00d9\u00e0\u00e3\u00e8\3\b\2\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}